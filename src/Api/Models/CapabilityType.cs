namespace Api.Models;

public record class CapabilityType(string name, string actuator_mode)
{
    public static implicit operator CapabilityType(Core.Entities.CapabilityType capabilityType) => new(capabilityType.Name, capabilityType.ActuatorMode);

    public static implicit operator Core.Entities.CapabilityType(CapabilityType capabilityType) => new(capabilityType.name, capabilityType.actuator_mode);
}
public record class Platform(string name, string? description)
{
    public static implicit operator Platform(Core.Entities.Platform platform) => new(platform.Name, platform.Description);

    public static implicit operator Core.Entities.Platform(Platform platform) => new(platform.name, platform.description);
}