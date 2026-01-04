
namespace Core.Entities;

public class Capability
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Type { get; set; } = default!;
    public string Mode { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Owner { get; set; }
    public string DeviceId { get; set; }
    public bool Active { get; set; } = true;
    public string? IconName { get; set; }
    public string? IconActiveColor { get; set; }
    public string? IconInactiveColor { get; set; }

    public string? DataType { get; set; } = default!;
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<CapabilityPlatform> Platforms { get; set; } = [];
    public IEnumerable<CapabilityGroup> Groups { get; set; } = [];
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
        if (!string.IsNullOrWhiteSpace(DataType))
        {
            var dt = new CapabilityDataType(DataType);
            Value = dt.Convert(value.ToLower());
        }
        else
        {
            Value = value;
        }
    }

    public bool HasGroups(string name)
    {
        return Groups.Any(g => g.Name == name);
    }

    public void AddGroup(CapabilityGroup group)
    {
        if (group is null || HasGroups(group.Name))
            return;

        Groups = Groups.Append(group);
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
