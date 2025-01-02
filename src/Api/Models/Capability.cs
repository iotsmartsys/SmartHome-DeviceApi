namespace Api.Models;

public record class Capability(string capability_name, string? description, string? owner, string type, string mode, string value)
{
    public static implicit operator Capability(Core.Entities.Capability capability) => new(capability.Name, capability.Description, capability.Owner, capability.Type, capability.Mode, capability.Value);

    public static implicit operator Core.Entities.Capability(Capability capability) => new()
    {
        Name = capability.capability_name,
        Owner = capability.owner,
        Type = capability.type,
        Mode = capability.mode,
        Value = capability.value,
        Description = capability.description
    };
}
