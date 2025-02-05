namespace Api.Models;

public record class Capability(string capability_name, string? description, string? owner, string type, string? mode, string? value, IEnumerable<string>? platforms,string? value_type, string? updated_at)
{
    public static implicit operator Capability(Core.Entities.Capability capability) => new(capability.Name, capability.Description, capability.Owner, capability.Type, capability.Mode, capability.Value, capability.Platforms, capability.DataType, capability.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

    public static implicit operator Core.Entities.Capability(Capability capability) => new()
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