namespace Api.Models;

public record class Property(string name, string value)
{
    public static implicit operator Property(Core.Entities.Property property) => new(property.Name, property.Value);

    public static implicit operator Core.Entities.Property(Property property) => new()
    {
        Name = property.name,
        Value = property.value
    };
}