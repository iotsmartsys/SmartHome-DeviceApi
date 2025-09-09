namespace Api.Models;

public class CapabilityIcon
{
    public string name { get; set; } = default!;
    public string? primary_color { get; set; }
    public string? secondary_color { get; set; }

    public CapabilityIcon() { }

    public CapabilityIcon(string name, string? primary_color, string? secondary_color)
    {
        this.name = name;
        this.primary_color = primary_color;
        this.secondary_color = secondary_color;
    }
}
