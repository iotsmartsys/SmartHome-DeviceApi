namespace Api.Models;

public record class CapabilityType(string name, string actuator_mode, string data_type, bool computed_value)
{
    public static implicit operator CapabilityType(Core.Entities.CapabilityType capabilityType) => new(capabilityType.Name, capabilityType.ActuatorMode, capabilityType.DataType, capabilityType.ComputedValue);

    public static implicit operator Core.Entities.CapabilityType(CapabilityType capabilityType) => new(capabilityType.name, capabilityType.actuator_mode, capabilityType.data_type, capabilityType.computed_value);
}