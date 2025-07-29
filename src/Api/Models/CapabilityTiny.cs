namespace Api.Models;

public record class CapabilityTiny(string capability_name, string? owner, string value)
{
    public static implicit operator CapabilityTiny(Core.Entities.Capability capability) => new CapabilityTiny(capability.Name, capability.Owner, capability.Value);
}