
namespace Api.Models;

public record class Group(int id, string name, bool active, IconGroup? icon, IEnumerable<Capability> capabilities)
{
    public static implicit operator Group(Core.Entities.Group group)
    {
        IconGroup? icon = group.Icon is null ? null : (IconGroup)group.Icon;
        var capabilities = group.Capabilities.Select(c => (Capability)c!).ToList();
        return new(group.Id, group.Name, group.IsActive, icon, capabilities!);
    }

    public static implicit operator Core.Entities.Group(Group group)
    {
        Core.Entities.IconGroup? icon = group.icon is null ? null : (Core.Entities.IconGroup)group.icon;
        Core.Entities.Group entity = new()
        {
            Id = group.id,
            Name = group.name,
            IsActive = group.active,
            Icon = icon
        };

        foreach (var capability in group.capabilities)
        {
            entity.AddCapability(capability);
        }

        return entity;
    }
}
public record class IconGroup(string name)
{
    public static implicit operator IconGroup(Core.Entities.IconGroup icon)
    {
        return new(icon.Name);
    }

    public static implicit operator Core.Entities.IconGroup(IconGroup iconGroup)
    {
        return new Core.Entities.IconGroup
        {
            Name = iconGroup.name
        };
    }
}
public record class CapabilityGroup(int id, string capability_name, string? description = null)
{
    public static implicit operator CapabilityGroup(Core.Entities.CapabilityGroup group)
    {
        return new(group.Id, group.Name);
    }

    public static implicit operator Core.Entities.CapabilityGroup(CapabilityGroup capabilityGroup)
    {
        return new Core.Entities.CapabilityGroup
        {
            Id = capabilityGroup.id,
            Name = capabilityGroup.capability_name,
        };
    }
}