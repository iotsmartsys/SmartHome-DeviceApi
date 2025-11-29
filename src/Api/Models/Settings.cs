
namespace Api.Models;

public record class Settings(string? name, string? value, string? description) : ISelfValidate
{
    public static implicit operator Settings(Core.Entities.Settings settings) => new(settings.Name, settings.Value, settings.Description);

    public static implicit operator Core.Entities.Settings(Settings settings)
    {
        Core.Entities.Settings entity = new()
        {
            Name = settings.name!,
            Value = settings.value!,
            Description = settings.description
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

