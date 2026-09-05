using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Core.Entities;

namespace Core.Services;

public sealed class DashboardDataResolver(string? sourceTimeZone)
{
    private static readonly Regex Numeric = new(@"^[+-]?(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?$", RegexOptions.CultureInvariant);
    private static readonly Regex Integer = new(@"^[+-]?[0-9]+$", RegexOptions.CultureInvariant);
    private static readonly Regex Instant = new(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]+)?(?:Z|[+-][0-9]{2}:[0-9]{2})$", RegexOptions.CultureInvariant);

    public DashboardReading Resolve(DashboardCapabilitySource? source, DashboardWidget? widget,
        IEnumerable<DashboardWidgetType> types, DateTimeOffset now)
    {
        if (source is null) return new(null, null, null, null, "capability_missing", null);
        var unit = string.IsNullOrEmpty(source.Unit) ? null : source.Unit;
        DateTimeOffset? updated = null;
        try
        {
            var config = widget?.Config;
            unit = config?["unit"]?.GetValue<string>() ?? unit;
            var timeStatus = ReadTime(source.UpdatedAt, out updated);
            if (timeStatus == "error") return Empty("error");
            if (!source.DeviceExists || !source.DeviceActive || !source.Active ||
                string.Equals(source.DeviceState?.Trim(), "offline", StringComparison.OrdinalIgnoreCase)) return Empty("offline");
            if (string.IsNullOrWhiteSpace(source.Value) || !source.UpdatedAt.HasValue || source.UpdatedAt == default(DateTime)) return Empty("no_data");
            if (timeStatus == "invalid_value" || updated > now) return Empty("invalid_value");
            var visual = DashboardWidgetCompatibilityResolver.VisualType(source);
            var type = widget is null ? null : types.SingleOrDefault(t => t.Code == widget.WidgetType);
            if (visual is null || widget is not null && (type is null || !type.Enabled ||
                    !type.CompatibleDataTypes.Contains(visual, StringComparer.Ordinal) || widget.DataMode != "current_value"))
                return Empty("invalid_value");
            if (!Convert(source, out var value)) return Empty("invalid_value");
            var status = (now - updated!.Value).TotalSeconds > 300 ? "stale" : "ok";
            var (label, icon) = Present(source, value!, config, unit);
            return new(value, unit, label, icon, status, updated);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // Corrupt data/configuration in a single widget must not remove the other widgets.
            return Empty("error");
        }
        DashboardReading Empty(string status) => new(null, unit, null, null, status, updated);
    }

    private string? ReadTime(DateTime? stored, out DateTimeOffset? updated)
    {
        updated = null;
        if (!stored.HasValue || stored.Value == default) return null;
        var time = stored.Value;
        if (time.Kind == DateTimeKind.Utc) { updated = new DateTimeOffset(time); return null; }
        if (time.Kind == DateTimeKind.Local) { updated = new DateTimeOffset(time).ToUniversalTime(); return null; }
        TimeZoneInfo zone;
        try
        {
            if (string.IsNullOrWhiteSpace(sourceTimeZone)) return "error";
            zone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZone);
            if (!zone.HasIanaId) return "error";
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { return "error"; }
        if (zone.IsAmbiguousTime(time) || zone.IsInvalidTime(time)) return "invalid_value";
        try { updated = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(time, zone)); }
        catch (ArgumentException) { return "invalid_value"; }
        return null;
    }

    private static bool Convert(DashboardCapabilitySource source, out object? value)
    {
        value = null;
        var raw = source.Value!.Trim();
        var token = raw.ToLowerInvariant();
        switch (DashboardWidgetCompatibilityResolver.SourceType(source))
        {
            case "float":
                if (!Numeric.IsMatch(raw) || !double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || !double.IsFinite(number)) return false;
                value = number; return true;
            case "integer":
                if (!Integer.IsMatch(raw) || !long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integer)) return false;
                value = integer; return true;
            case "boolean":
                if (token is not ("true" or "false")) return false;
                value = token == "true"; return true;
            case "detection":
                if (token is not ("true" or "false" or "detected" or "undetected")) return false;
                value = token is "true" or "detected"; return true;
            case "open_closed":
                value = token switch { "true" or "closed" => "closed", "false" or "open" => "open", _ => null }; break;
            case "on_off": case "power":
                value = token switch { "true" or "on" => "on", "false" or "off" => "off", _ => null }; break;
            case "press":
                value = token switch { "true" or "pressed" => "pressed", "false" or "released" => "released", _ => null }; break;
            case "text": value = source.Value; break;
            case "time":
                if (!Instant.IsMatch(raw) || !DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var instant)) return false;
                value = instant.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture); break;
        }
        return value is not null;
    }

    private static (string? Label, string? Icon) Present(DashboardCapabilitySource source, object value, JsonObject? config, string? unit)
    {
        var visual = DashboardWidgetCompatibilityResolver.VisualType(source);
        if (visual == "numeric")
        {
            var decimals = config?["decimals"]?.GetValue<int>() ?? 1;
            var format = "F" + decimals.ToString(CultureInfo.InvariantCulture);
            // Decimal source text preserves decimal midpoint rounding (e.g. 1.005 -> 1.01).
            // The double fallback retains the full finite range of float readings.
            var label = value is long integer ? integer.ToString(format, CultureInfo.InvariantCulture)
                : decimal.TryParse(source.Value!.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var precise)
                    ? Math.Round(precise, decimals, MidpointRounding.AwayFromZero).ToString(format, CultureInfo.InvariantCulture)
                    : Math.Round((double)value, decimals, MidpointRounding.AwayFromZero).ToString(format, CultureInfo.InvariantCulture);
            return (label + (unit is null ? "" : " " + unit), null);
        }
        if (visual is "text" or "event") return ((string)value, null);
        if (config is null) return (value.ToString(), null); // Metadata list does not expose presentation fields.
        var invert = config["invertState"]!.GetValue<bool>();
        string key;
        if (visual == "logical") key = (bool)value ^ invert ? "on" : "off";
        else
        {
            key = (string)value;
            if (invert) key = key switch
            {
                "open" => "closed", "closed" => "open", "on" => "off", "off" => "on",
                "pressed" => "released", "released" => "pressed", _ => key
            };
        }
        return (config[key + "Label"]?.GetValue<string>(), config[key + "Icon"]?.GetValue<string>());
    }
}
