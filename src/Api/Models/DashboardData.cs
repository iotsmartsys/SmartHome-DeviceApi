using Newtonsoft.Json;

namespace Api.Models;

public sealed record DashboardData(long dashboardId, string name, string layoutType, string generatedAt,
    IEnumerable<DashboardWidgetData> widgets);

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record DashboardWidgetData(long widgetId, string? title, string? deviceId, int capabilityId,
    string? capabilityCode, string widgetType, string? dataType, string dataMode, object? value, string? unit,
    string? label, string? icon, string status, string? lastUpdatedAt, DashboardPosition position,
    DashboardWidgetConfig config, int displayOrder, int? refreshIntervalSeconds)
{
    public static DashboardWidgetData FromEntity(Core.Entities.DashboardWidget entity, Core.Entities.DashboardCapability? capability,
        Core.Entities.DashboardReading reading) =>
        new(entity.Id, entity.Title, capability?.DeviceId, entity.CapabilityId, capability?.CapabilityCode, entity.WidgetType,
            capability?.DataType, entity.DataMode, reading.Value, reading.Unit, reading.Label, reading.Icon, reading.Status,
            DashboardTimestamp.FromInstant(reading.LastUpdatedAt), entity, entity.Config, entity.DisplayOrder, entity.RefreshIntervalSeconds);
}
