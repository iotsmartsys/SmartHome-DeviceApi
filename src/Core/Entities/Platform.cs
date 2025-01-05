namespace Core.Entities;

public class Platform
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Platform(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public Platform() { }
}
