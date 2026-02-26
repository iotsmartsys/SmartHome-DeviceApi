namespace Api.Models;

public record class SmartHomeCapability(
    string capability_name
    , string uid
    , string? description
    , string type
    , string? mode
    , string? value
    , bool active)
{
    public IDictionary<string, object> smart_home = new Dictionary<string, object>();
    public static implicit operator SmartHomeCapability(Core.Entities.Capability capability)
    {
        IDictionary<string, object> smartHomeTypes = capability.SmartHomeTypes
            .Where(x => !string.IsNullOrWhiteSpace(x.SmartHomeId))
            .GroupBy(x => x.SmartHomeId)
            .ToDictionary(
                group => group.Key,
                group => (object)BuildSmartHomePayload(group.OrderBy(x => x.Id))
            );

        return new SmartHomeCapability(
          capability.Name
        , capability.UID
        , capability.Description
        , capability.Type
        , capability.Mode
        , capability.Value
        , capability.Active)
        {
            smart_home = smartHomeTypes
        };
    }

    private static IDictionary<string, object> BuildSmartHomePayload(IEnumerable<Core.Entities.CapabilityTypeSmartHome> entries)
    {
        var payload = new Dictionary<string, object>();

        foreach (var item in entries.Where(x => string.IsNullOrWhiteSpace(x.Parent)))
            payload[item.Name] = item.Value;

        foreach (var parentGroup in entries
            .Where(x => !string.IsNullOrWhiteSpace(x.Parent))
            .GroupBy(x => x.Parent!))
        {
            var withGroup = parentGroup.Where(x => !string.IsNullOrWhiteSpace(x.Group)).ToList();
            if (withGroup.Count > 0)
            {
                payload[parentGroup.Key] = withGroup
                    .GroupBy(x => x.Group!)
                    .Select(group => (object)group.ToDictionary(x => x.Name, x => (object)x.Value))
                    .ToList();
                continue;
            }

            payload[parentGroup.Key] = parentGroup
                .ToDictionary(x => x.Name, x => (object)x.Value);
        }

        return payload;
    }
}
