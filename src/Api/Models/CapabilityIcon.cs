namespace Api.Models;

public class CapabilityIcon
{
    public string name { get; set; } = default!;
    public string? active_color { get; set; }
    public string? inactive_color { get; set; }

    public CapabilityIcon() { }

    public CapabilityIcon(string name, string? active_color, string? inactive_color)
    {
        this.name = name;
        this.active_color = active_color;
        this.inactive_color = inactive_color;
    }
}
