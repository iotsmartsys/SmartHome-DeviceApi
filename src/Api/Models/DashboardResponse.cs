using System.Globalization;
using Core.Entities;
using Core.Services;

namespace Api.Models;

internal static class DashboardResponse
{
    internal static object Dashboard(Core.Entities.Dashboard dashboard, IReadOnlyDictionary<int, DashboardCapabilitySource> sources) => new
    {
        dashboard.Id, dashboard.Name, dashboard.Description, dashboard.LayoutType, dashboard.IsDefault,
        dashboard.DisplayOrder, CreatedAt = Utc(dashboard.CreatedAt), UpdatedAt = Utc(dashboard.UpdatedAt),
        Widgets = dashboard.Widgets.Select(w => Widget(w, sources.GetValueOrDefault(w.CapabilityId))).ToArray()
    };
    internal static object Summary(Core.Entities.Dashboard dashboard) => new
    {
        dashboard.Id, dashboard.Name, dashboard.Description, dashboard.LayoutType, dashboard.IsDefault,
        dashboard.DisplayOrder, CreatedAt = Utc(dashboard.CreatedAt), UpdatedAt = Utc(dashboard.UpdatedAt),
        WidgetCount = dashboard.Widgets.Count
    };
    internal static object Widget(DashboardWidget widget, DashboardCapabilitySource? source) => new
    {
        widget.Id, widget.DashboardId, widget.Title, widget.CapabilityId, DeviceId = source?.DeviceId,
        CapabilityCode = source?.CapabilityCode, DataType = DashboardWidgetCompatibilityResolver.VisualType(source),
        widget.WidgetType, widget.DataMode, Position = Position(widget), widget.Config,
        widget.RefreshIntervalSeconds, widget.DisplayOrder, CreatedAt = Utc(widget.CreatedAt), UpdatedAt = Utc(widget.UpdatedAt)
    };
    internal static object Type(DashboardWidgetType type) => new
    {
        type.Code, type.Name, type.Description, type.CompatibleDataTypes, type.DefaultDataMode,
        type.DefaultConfig, type.Enabled, type.Lifecycle
    };
    internal static object Capability(DashboardCapabilitySource source, DashboardReading reading, IReadOnlyList<DashboardWidgetType> compatible) => new
    {
        source.DeviceId, source.DeviceName, source.CapabilityId, source.CapabilityCode, source.CapabilityName,
        DataType = DashboardWidgetCompatibilityResolver.VisualType(source), reading.Unit,
        SemanticType = DashboardWidgetCompatibilityResolver.SemanticType(source), CurrentValue = reading.Value,
        LastUpdatedAt = Instant(reading.LastUpdatedAt), reading.Status, CompatibleWidgets = compatible.Select(t => t.Code).ToArray()
    };
    internal static object RenderedWidget(DashboardWidget widget, DashboardCapabilitySource? source, DashboardReading reading) => new
    {
        WidgetId = widget.Id, widget.Title, DeviceId = source?.DeviceId, widget.CapabilityId,
        CapabilityCode = source?.CapabilityCode, widget.WidgetType,
        DataType = DashboardWidgetCompatibilityResolver.VisualType(source), widget.DataMode,
        reading.Value, reading.Unit, reading.Label, reading.Icon, reading.Status,
        LastUpdatedAt = Instant(reading.LastUpdatedAt), Position = Position(widget), widget.Config,
        widget.DisplayOrder, widget.RefreshIntervalSeconds
    };
    private static object Position(DashboardWidget widget) => new { widget.X, widget.Y, widget.Width, widget.Height };
    private static string? Utc(DateTime? value) => value.HasValue ? Instant(new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))) : null;
    internal static string? Instant(DateTimeOffset? value) => value?.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture);
}
