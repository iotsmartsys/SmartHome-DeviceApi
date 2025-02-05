namespace Core.Entities;

public class Property
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Description { get; set; }

    public Property(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public Property() { }

    public override string ToString() => $"Property: {Name} - {Value}";
}