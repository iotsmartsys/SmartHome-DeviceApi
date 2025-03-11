using Core.Entities;

namespace Core.Contracts.Events;

public record CapabilityRegisterEvent(string action, string context, IEnumerable<CapabilityEventModel> capabilities);
public record class CapabilityEventModel(string capability_name, string? description, string? owner, string type, string? mode, string? value, IEnumerable<string>? platforms, string? value_type)
{
    public static implicit operator CapabilityEventModel(Capability capability) => new(capability.Name, capability.Description, capability.Owner, capability.Type, capability.Mode, capability.Value, capability.Platforms, capability.DataType);

    public static implicit operator Capability(CapabilityEventModel capability) => new()
    {
        Name = capability.capability_name,
        Owner = capability.owner,
        Type = capability.type,
        Mode = capability.mode ?? default!,
        Value = capability.value!,
        Description = capability.description,
        Platforms = capability.platforms ?? [],
        DataType = capability.value_type!
    };
}

public record class CapabilityCommand(string device_id, string capability_name, string value);