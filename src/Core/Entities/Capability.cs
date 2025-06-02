
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
    public IEnumerable<string> Platforms { get; set; } = [];
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
        if (string.IsNullOrWhiteSpace(platform) || Platforms.Contains(platform))
            return this;

        Platforms = Platforms.Append(platform);
        return this;
    }
    public void UpdateValue(string value)
    {
        Value = DataType?.Convert(value.ToLower()) ?? value;
    }
}