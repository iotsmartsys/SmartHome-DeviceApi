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
    public Capability(string name, string type, string mode, string value, string owner)
    {
        Name = name;
        Type = type;
        Mode = mode;
        Value = value;
        Owner = owner;
    }

    public Capability() { }
}
