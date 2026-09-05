using System.Text.Json.Nodes;
using Core.Contracts.Repositories;
using Core.Entities;
using Core.Exceptions;
using static Core.Services.DashboardInput;

namespace Core.Services;

public sealed class DashboardService(IDashboardRepository repository, DashboardWidgetCompatibilityResolver compatibility)
{
    private static readonly string[] DashboardFields = ["name", "description", "layoutType", "isDefault", "displayOrder"];
    private static readonly string[] WidgetFields = ["title", "deviceId", "capabilityId", "widgetType", "dataMode", "position", "config", "refreshIntervalSeconds", "displayOrder"];
    private static readonly string[] StateConfigFields = ["unit", "invertState", "onLabel", "offLabel", "onIcon", "offIcon", "openLabel", "closedLabel", "openIcon", "closedIcon", "pressedLabel", "releasedLabel", "pressedIcon", "releasedIcon"];

    public Task<IReadOnlyList<Dashboard>> ListAsync(CancellationToken ct) => repository.GetAllAsync(ct);
    public async Task<Dashboard> GetAsync(long id, CancellationToken ct) =>
        await repository.GetAsync(id, ct) ?? throw Missing("DASHBOARD_NOT_FOUND");
    public Task<IReadOnlyList<DashboardWidgetType>> TypesAsync(CancellationToken ct) => repository.GetWidgetTypesAsync(ct);
    public Task<IReadOnlyList<DashboardCapabilitySource>> SourcesAsync(CancellationToken ct) => repository.GetSourcesAsync(ct);
    public async Task<DashboardCapabilitySource> SourceAsync(int id, CancellationToken ct) =>
        await repository.GetSourceAsync(id, ct) ?? throw Missing("CAPABILITY_NOT_FOUND");

    public Task<Dashboard> SaveDashboardAsync(long? id, JsonObject input, CancellationToken ct) =>
        repository.WithWriteLockAsync(async () =>
        {
            var value = id.HasValue ? await GetAsync(id.Value, ct) : new Dashboard { CreatedAt = UtcNow() };
            Fields(input, DashboardFields);
            var old = DashboardState(value);
            if (!id.HasValue || input.ContainsKey("name"))
                value.Name = Text(input["name"], "name", "INVALID_DASHBOARD_NAME", 120, required: true)!;
            if (input.ContainsKey("description"))
                value.Description = Text(input["description"], "description", "INVALID_DASHBOARD_DESCRIPTION", 255, trim: false);
            if (input.ContainsKey("layoutType"))
                value.LayoutType = Literal(input["layoutType"], "layoutType", "grid", "INVALID_LAYOUT_TYPE");
            if (input.ContainsKey("isDefault"))
                value.IsDefault = input["isDefault"] is { } flag && Boolean(flag, "isDefault", "INVALID_REQUEST");
            if (input.ContainsKey("displayOrder")) value.DisplayOrder = Order(input["displayOrder"]);
            if (!id.HasValue || old != DashboardState(value))
            {
                if (id.HasValue) value.UpdatedAt = UtcNow();
                await repository.SaveAsync(value, !id.HasValue, ct);
            }
            return value;
        }, ct);

    public Task<DashboardWidget> SaveWidgetAsync(long dashboardId, long? widgetId, JsonObject input, CancellationToken ct) =>
        repository.WithWriteLockAsync(async () =>
        {
            var dashboard = await GetAsync(dashboardId, ct);
            var widget = widgetId.HasValue
                ? dashboard.Widgets.SingleOrDefault(w => w.Id == widgetId) ?? throw Missing("WIDGET_NOT_FOUND")
                : new DashboardWidget { DashboardId = dashboardId, CreatedAt = UtcNow() };
            Fields(input, WidgetFields);
            var before = WidgetState(widget);
            var oldConfig = widget.Config;
            if (!widgetId.HasValue || input.ContainsKey("capabilityId"))
                widget.CapabilityId = Integer(input["capabilityId"], "capabilityId", "INVALID_REQUEST", 1, int.MaxValue);
            if (!widgetId.HasValue || input.ContainsKey("widgetType"))
                widget.WidgetType = Text(input["widgetType"], "widgetType", "INVALID_REQUEST", 80, required: true, trim: false)!;
            var types = await TypesAsync(ct);
            var type = types.SingleOrDefault(t => t.Code == widget.WidgetType) ?? throw Missing("WIDGET_TYPE_NOT_FOUND");
            // Disabled entries stay unusable even if their planned default mode is historical.
            if (!type.Enabled) throw new DashboardException("WIDGET_TYPE_DISABLED", 422, "Tipo de widget desabilitado.", "widgetType");
            var source = await SourceAsync(widget.CapabilityId, ct);
            if (input.TryGetPropertyValue("deviceId", out var deviceNode) && deviceNode is not null)
            {
                var deviceId = Text(deviceNode, "deviceId", "INVALID_REQUEST", int.MaxValue, required: true, trim: false)!;
                if (!await repository.DeviceExistsAsync(deviceId, ct)) throw Missing("DEVICE_NOT_FOUND");
                if (!string.Equals(deviceId, source.DeviceId, StringComparison.Ordinal))
                    throw new DashboardException("DEVICE_CAPABILITY_MISMATCH", 422, "Device não pertence à capability.", "deviceId");
            }
            compatibility.Validate(source, type);
            if (input.ContainsKey("title")) widget.Title = Text(input["title"], "title", "INVALID_WIDGET_TITLE", 120);
            if (!widgetId.HasValue) widget.DataMode = type.DefaultDataMode;
            if (input.ContainsKey("dataMode")) widget.DataMode = Literal(input["dataMode"], "dataMode", "current_value", "INVALID_DATA_MODE");
            if (widget.DataMode != "current_value") throw Invalid("INVALID_DATA_MODE", "dataMode");
            if (input.ContainsKey("displayOrder")) widget.DisplayOrder = Order(input["displayOrder"]);
            if (input.ContainsKey("refreshIntervalSeconds"))
                widget.RefreshIntervalSeconds = input["refreshIntervalSeconds"] is { } refresh
                    ? Integer(refresh, "refreshIntervalSeconds", "INVALID_REFRESH_INTERVAL", 1, 86400) : null;
            if (input.ContainsKey("position")) Position(widget, input["position"]);
            var config = widgetId.HasValue && !input.ContainsKey("config") ? widget.Config : MergeConfig(type, input["config"]);
            ValidateConfig(widget.WidgetType, config);
            widget.ConfigJson = config.ToJsonString();
            if (!widgetId.HasValue || before != WidgetState(widget) || !JsonNode.DeepEquals(oldConfig, config))
            {
                if (widgetId.HasValue) widget.UpdatedAt = UtcNow();
                await repository.SaveWidgetAsync(widget, !widgetId.HasValue, ct);
            }
            return widget;
        }, ct);

    public Task<bool> DeleteDashboardAsync(long id, CancellationToken ct) => repository.WithWriteLockAsync(async () =>
    {
        await GetAsync(id, ct);
        await repository.DeleteAsync(id, ct);
        return true;
    }, ct);
    public Task<bool> DeleteWidgetAsync(long dashboardId, long widgetId, CancellationToken ct) => repository.WithWriteLockAsync(async () =>
    {
        var dashboard = await GetAsync(dashboardId, ct);
        if (!dashboard.Widgets.Any(w => w.Id == widgetId)) throw Missing("WIDGET_NOT_FOUND");
        await repository.DeleteWidgetAsync(dashboardId, widgetId, ct);
        return true;
    }, ct);

    // MySQL DATETIME(6) stores microseconds: POST and subsequent GET must agree.
    private static DateTime UtcNow()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
    }
    private static DashboardException Missing(string code) => new(code, 404, "Recurso não encontrado.");
    private static int Order(JsonNode? value) => value is null ? 0 : Integer(value, "displayOrder", "INVALID_DISPLAY_ORDER", 0, int.MaxValue);
    private static string Literal(JsonNode? value, string field, string expected, string code)
    {
        if (value is null) return expected;
        if (value is not JsonValue v || !v.TryGetValue<string>(out var text) || text != expected) throw Invalid(code, field);
        return expected;
    }
    private static void Position(DashboardWidget w, JsonNode? node)
    {
        if (node is null) { w.X = w.Y = 0; w.Width = w.Height = 1; return; }
        var position = Object(node, "position", "INVALID_WIDGET_POSITION");
        Fields(position, ["x", "y", "width", "height"], "INVALID_WIDGET_POSITION", "position.");
        foreach (var (key, value) in position)
        {
            var dimension = key is "width" or "height";
            var n = value is null ? (dimension ? 1 : 0) : Integer(value, "position." + key, "INVALID_WIDGET_POSITION", dimension ? 1 : 0, dimension ? 4 : int.MaxValue);
            switch (key) { case "x": w.X = n; break; case "y": w.Y = n; break; case "width": w.Width = n; break; case "height": w.Height = n; break; }
        }
    }
    private static JsonObject MergeConfig(DashboardWidgetType type, JsonNode? input)
    {
        var defaults = type.DefaultConfig;
        if (input is null) return defaults;
        var supplied = Object(input, "config", "INVALID_WIDGET_CONFIG");
        foreach (var (key, value) in supplied)
        {
            if (!defaults.ContainsKey(key)) throw Invalid("INVALID_WIDGET_CONFIG", "config." + key);
            if (value is not null) defaults[key] = value.DeepClone();
        }
        return defaults;
    }
    private static void ValidateConfig(string type, JsonObject config)
    {
        string[] fields = type switch
        {
            "gauge" => ["unit", "min", "max", "warningFrom", "dangerFrom", "decimals"],
            "value_card" => ["unit", "decimals", "showLastUpdated"],
            "state_icon" or "status_card" => StateConfigFields,
            _ => throw Invalid("INVALID_WIDGET_CONFIG", "widgetType")
        };
        Fields(config, fields, "INVALID_WIDGET_CONFIG", "config.");
        // Persisted configuration must remain complete when a PUT changes the widget type.
        if (fields.Any(f => !config.ContainsKey(f))) throw Invalid("INVALID_WIDGET_CONFIG", "config");
        foreach (var (key, node) in config.ToArray())
        {
            var field = "config." + key;
            if (key == "unit") config[key] = Text(node, field, "INVALID_WIDGET_CONFIG", 32);
            else if (key.EndsWith("Label", StringComparison.Ordinal)) config[key] = Text(node, field, "INVALID_WIDGET_CONFIG", 120, required: true);
            else if (key.EndsWith("Icon", StringComparison.Ordinal)) config[key] = Text(node, field, "INVALID_WIDGET_CONFIG", 120);
            else if (key is "showLastUpdated" or "invertState") Boolean(node, field, "INVALID_WIDGET_CONFIG");
            else if (key == "decimals") Integer(node, field, "INVALID_WIDGET_CONFIG", 0, 6);
        }
        if (type != "gauge") return;
        var min = Number(config["min"], "config.min");
        var max = Number(config["max"], "config.max");
        if (min >= max) throw Invalid("INVALID_WIDGET_CONFIG", "config.min");
        double? warning = config["warningFrom"] is { } warningNode ? Number(warningNode, "config.warningFrom") : null;
        double? danger = config["dangerFrom"] is { } dangerNode ? Number(dangerNode, "config.dangerFrom") : null;
        if (warning.HasValue && (warning < min || warning > max)) throw Invalid("INVALID_WIDGET_CONFIG", "config.warningFrom");
        if (danger.HasValue && (danger < min || danger > max)) throw Invalid("INVALID_WIDGET_CONFIG", "config.dangerFrom");
        if (warning > danger) throw Invalid("INVALID_WIDGET_CONFIG", "config.warningFrom");
    }
    private static string DashboardState(Dashboard d) => System.Text.Json.JsonSerializer.Serialize(new { d.Name, d.Description, d.LayoutType, d.IsDefault, d.DisplayOrder });
    private static string WidgetState(DashboardWidget w) => System.Text.Json.JsonSerializer.Serialize(new
    { w.CapabilityId, w.Title, w.WidgetType, w.DataMode, w.X, w.Y, w.Width, w.Height, w.RefreshIntervalSeconds, w.DisplayOrder });
}
