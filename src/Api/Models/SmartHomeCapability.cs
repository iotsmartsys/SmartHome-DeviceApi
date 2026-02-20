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
                group => (object)group
                    .OrderBy(x => x.Id)
                    .GroupBy(x => x.Name)
                    .ToDictionary(nameGroup => nameGroup.Key, nameGroup => nameGroup.First().Value)
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
}
