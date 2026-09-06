using Newtonsoft.Json;

namespace Api.Models;

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record Dashboard(long id, string name, string? description, string layoutType, bool isDefault,
    int displayOrder, string createdAt, string? updatedAt, IEnumerable<DashboardWidget> widgets)
{
    public static Dashboard FromEntity(Core.Entities.Dashboard entity, IReadOnlyDictionary<int, Core.Entities.DashboardCapability> capabilities) =>
        new(entity.Id, entity.Name, entity.Description, entity.LayoutType, entity.IsDefault, entity.DisplayOrder,
            DashboardTimestamp.FromUtc(entity.CreatedAt)!, DashboardTimestamp.FromUtc(entity.UpdatedAt),
            entity.Widgets.Select(widget => DashboardWidget.FromEntity(widget, capabilities.GetValueOrDefault(widget.CapabilityId))).ToArray());
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record DashboardSummary(long id, string name, string? description, string layoutType, bool isDefault,
    int displayOrder, string createdAt, string? updatedAt, int widgetCount)
{
    public static implicit operator DashboardSummary(Core.Entities.Dashboard entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.LayoutType, entity.IsDefault, entity.DisplayOrder,
            DashboardTimestamp.FromUtc(entity.CreatedAt)!, DashboardTimestamp.FromUtc(entity.UpdatedAt), entity.Widgets.Count());
}

public sealed record DashboardsResponse(IEnumerable<DashboardSummary> items);

internal static class DashboardTimestamp
{
    internal static string? FromUtc(DateTime? value) => value.HasValue
        ? FromInstant(new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))) : null;
    internal static string? FromInstant(DateTimeOffset? value) => value?.ToUniversalTime()
        .ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", System.Globalization.CultureInfo.InvariantCulture);
}
