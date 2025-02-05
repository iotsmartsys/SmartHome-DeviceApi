namespace Api.Models;

public record class CapabilityUpdate(string capability_name, string value)
{
    public static implicit operator Core.Entities.Capability(CapabilityUpdate capability) => new()
    {
        Name = capability.capability_name,
        Value = capability.value
    };
}
