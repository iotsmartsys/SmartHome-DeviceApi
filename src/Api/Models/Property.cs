
namespace Api.Models;

public record class Property(string? name, string? value, string? description) : ISelfValidate
{
    public int Id { get; init; }
    public static implicit operator Property(Core.Entities.Property property) => new(property.Name, property.Value, property.Description)
    {
        Id = property.Id
    };

    public static implicit operator Core.Entities.Property(Property property)
    {
        Core.Entities.Property entity = new()
        {
            Name = property.name!,
            Value = property.value!,
            Description = property.description
        };

        return entity;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentDomainException("Name é obrigatório", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentDomainException("Value é obrigatório", nameof(value));
    }
}
