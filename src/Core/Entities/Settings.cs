namespace Core.Entities;

public class Settings
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Description { get; set; }

    public Settings(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public Settings() { }

    public override string ToString() => $"Settings: {Name} - {Value}";
}
