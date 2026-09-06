using Newtonsoft.Json;

namespace Api.Models;

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record DashboardCapability(string? deviceId, string? deviceName, int capabilityId, string? capabilityCode,
    string? capabilityName, string? dataType, string? unit, string? semanticType, object? currentValue,
    string? lastUpdatedAt, string status, IEnumerable<string> compatibleWidgets)
{
    public static DashboardCapability FromEntity(Core.Entities.DashboardCapability entity, Core.Entities.DashboardReading reading,
        IEnumerable<Core.Entities.DashboardWidgetType> compatible) =>
        new(entity.DeviceId, entity.DeviceName, entity.CapabilityId, entity.CapabilityCode, entity.CapabilityName,
            entity.DataType, reading.Unit, entity.SemanticType, reading.Value, DashboardTimestamp.FromInstant(reading.LastUpdatedAt),
            reading.Status, compatible.Select(type => type.Code).ToArray());
}

public sealed record DashboardCapabilitiesResponse(IEnumerable<DashboardCapability> items);

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
public sealed record DashboardCompatibleWidgetsResponse(int capabilityId, string? capabilityCode,
    string? dataType, IEnumerable<DashboardWidgetType> compatibleWidgets);
