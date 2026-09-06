using Newtonsoft.Json;

namespace Api.Models;

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record DashboardWidget(long id, long dashboardId, string? title, int capabilityId, string? deviceId,
    string? capabilityCode, string? dataType, string widgetType, string dataMode, DashboardPosition position,
    DashboardWidgetConfig config, int? refreshIntervalSeconds, int displayOrder, string createdAt, string? updatedAt)
{
    public static DashboardWidget FromEntity(Core.Entities.DashboardWidget entity, Core.Entities.DashboardCapability? capability) =>
        new(entity.Id, entity.DashboardId, entity.Title, entity.CapabilityId, capability?.DeviceId, capability?.CapabilityCode,
            capability?.DataType, entity.WidgetType, entity.DataMode, entity, entity.Config, entity.RefreshIntervalSeconds,
            entity.DisplayOrder, DashboardTimestamp.FromUtc(entity.CreatedAt)!, DashboardTimestamp.FromUtc(entity.UpdatedAt));
}
