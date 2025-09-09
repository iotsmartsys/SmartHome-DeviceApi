using System.Collections.Generic;

namespace Core.Entities;

public class CapabilityType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string ActuatorMode { get; set; } = default!;
    public string DataType { get; set; } = default!;
    public bool ComputedValue { get; set; }
    public string? ValueSymbol { get; set; }
    public string? ActiveValue { get; set; }
    public IEnumerable<CapabilityIcon> Icons { get; set; } = new List<CapabilityIcon>();

    public CapabilityType() { }

    public CapabilityType(string name, string actuator_mode, string data_type, bool computed_value)
    {
        Name = name;
        ActuatorMode = actuator_mode;
        DataType = data_type;
        ComputedValue = computed_value;
    }
}
public class CapabilityIcon
{
    public string Name { get; set; } = default!;
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }

    public CapabilityIcon() { }

    public CapabilityIcon(string name, string? active_color, string? inactive_color)
    {
        Name = name;
        PrimaryColor = active_color;
        SecondaryColor = inactive_color;
    }
}
