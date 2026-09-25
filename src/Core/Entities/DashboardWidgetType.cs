namespace Core.Entities;

public class DashboardWidgetType
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public IEnumerable<string> CompatibleDataTypes { get; set; } = [];
    public string DefaultDataMode { get; set; } = "current_value";
    public DashboardWidgetConfig DefaultConfig { get; set; } = new();
    public bool Enabled { get; set; }
    public string Lifecycle { get; set; } = "available";
}
