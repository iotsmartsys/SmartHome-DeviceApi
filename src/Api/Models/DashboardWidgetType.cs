using Newtonsoft.Json;

namespace Api.Models;

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record DashboardWidgetType(string code, string name, string? description,
    IEnumerable<string> compatibleDataTypes, string defaultDataMode, DashboardWidgetConfig defaultConfig, bool enabled, string lifecycle)
{
    public static implicit operator DashboardWidgetType(Core.Entities.DashboardWidgetType entity) =>
        new(entity.Code, entity.Name, entity.Description, entity.CompatibleDataTypes, entity.DefaultDataMode,
            entity.DefaultConfig, entity.Enabled, entity.Lifecycle);
}

public sealed record DashboardWidgetTypesResponse(IEnumerable<DashboardWidgetType> items);
