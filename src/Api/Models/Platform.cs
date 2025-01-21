namespace Api.Models;

public record class Platform(string name, string? description)
{
    public static implicit operator Platform(Core.Entities.Platform platform) => new(platform.Name, platform.Description);

    public static implicit operator Core.Entities.Platform(Platform platform) => new(platform.name, platform.description!);
}