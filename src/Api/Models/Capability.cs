namespace Api.Models;

public record class Capability(string capability_name, string? description, string? owner, string type, string? mode, string? value, IEnumerable<CapabilityPlatform>? platforms, string? value_type, string? updated_at, bool active)
{
    public int Id { get; private set; }
    public static implicit operator Capability?(Core.Entities.Capability? capability)
    {
        if (capability is null)
            return null;

        var platforms = capability.Platforms?.Select(p => (CapabilityPlatform)p) ?? [];

        return new Capability(capability.Name, capability.Description, capability.Owner, capability.Type, capability.Mode, capability.Value, platforms, capability.DataType, capability.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"), capability.Active) { Id = capability.Id };
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
        Platforms = capability.platforms?.Select(p => (Core.Entities.CapabilityPlatform)p) ?? [],
    };
}
public record class CapabilityPlatform(string platform, string? referenceId)
{
    public static implicit operator CapabilityPlatform(Core.Entities.CapabilityPlatform platform) => new CapabilityPlatform(platform.Platform, platform.ReferenceId);

    public static implicit operator Core.Entities.CapabilityPlatform(CapabilityPlatform platform) => new Core.Entities.CapabilityPlatform(platform.platform, platform.referenceId);
}