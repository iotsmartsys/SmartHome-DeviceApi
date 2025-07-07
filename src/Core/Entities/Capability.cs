
namespace Core.Entities;

public class Capability
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Type { get; set; } = default!;
    public string Mode { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Owner { get; set; } = default!;
    public bool Active { get; set; } = true;
    public CapabilityDataType? DataType { get; set; } = default!;
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<CapabilityPlatform> Platforms { get; set; } = [];
    public Capability(string name, string type, string mode, string value, string owner, bool active)
    {
        Name = name;
        Type = type;
        Mode = mode;
        Value = value;
        Owner = owner;
        Active = active;
    }

    public Capability() { }

    public Capability AddPlatform(string platform)
    {
        if (string.IsNullOrWhiteSpace(platform) || Platforms.Any(x => x.Platform == platform))
            return this;

        Platforms = Platforms.Append(new(platform));
        return this;
    }

    public Capability AddPlatform(CapabilityPlatform platform)
    {
        if (platform is null || Platforms.Any(x => x.Platform == platform.Platform))
            return this;

        Platforms = Platforms.Append(platform);
        return this;
    }

    public void UpdateValue(string value)
    {
        Value = DataType?.Convert(value.ToLower()) ?? value;
    }
}
public class CapabilityPlatform
{
    public int Id { get; set; }
    public string Platform { get; set; } = default!;
    public string? ReferenceId { get; set; }
    public CapabilityPlatform(int id, string platform, string? referenceId = null)
    {
        Id = id;
        Platform = platform;
        ReferenceId = referenceId;
    }

    public CapabilityPlatform(string platform, string? referenceId = null)
    {
        Platform = platform;
        ReferenceId = referenceId;
    }
    public CapabilityPlatform() { }
}