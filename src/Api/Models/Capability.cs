namespace Api.Models;

public record class Capability(
    string capability_name
    , string? description
    , string? owner
    , string type
    , string? mode
    , string? value
    , IEnumerable<Capability.Platform>? platforms
    , string? value_type
    , string? updated_at
    , bool active
    , Capability.Icon? icon,
    IEnumerable<Capability.Group>? groups = null)
{
    public int Id { get; private set; }
    public static implicit operator Capability?(Core.Entities.Capability? capability)
    {
        if (capability is null)
            return null;

        var platforms = capability.Platforms?.Select(p => (Capability.Platform)p) ?? [];

        Capability.Icon? icon = capability.IconName is null ? null : new(capability.IconName, capability.IconActiveColor, capability.IconInactiveColor);
        return new Capability(capability.Name
        , capability.Description
        , capability.Owner
        , capability.Type
        , capability.Mode
        , capability.Value
        , platforms
        , capability.DataType
        , capability.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        , capability.Active
        , icon
        , capability.Groups?.Select(g => (Capability.Group)g) ?? [])
        {
            Id = capability.Id,
        };
    }

    public static implicit operator Core.Entities.Capability(Capability capability) => new()
    {
        Id = capability.Id,
        Name = capability.capability_name,
        Owner = capability.owner,
        Type = capability.type,
        Mode = capability.mode ?? default!,
        Value = capability.value!,
        Description = capability.description,
        DataType = capability.value_type!,
        Active = capability.active,
        IconName = capability.icon?.name,
        IconActiveColor = capability.icon?.active_color,
        IconInactiveColor = capability.icon?.inactive_color,
        Platforms = capability.platforms?.Select(p => (Core.Entities.CapabilityPlatform)p) ?? [],
    };

    public record class Platform(string platform, string? referenceId)
    {
        public static implicit operator Platform(Core.Entities.CapabilityPlatform platform) => new(platform.Platform, platform.ReferenceId);

        public static implicit operator Core.Entities.CapabilityPlatform(Capability.Platform platform) => new(platform.platform, platform.referenceId);
    }
    public record class Icon(string name, string? active_color, string? inactive_color);

    public record class Group(string name, string? iconName)
    {
        public static implicit operator Group(Core.Entities.CapabilityGroup group) => new(group.Name, group.IconName);

        public static implicit operator Core.Entities.CapabilityGroup(Group group) => new() { Name = group.name, IconName = group.iconName };
    }
}
