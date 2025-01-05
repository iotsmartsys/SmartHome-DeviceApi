namespace Core.Entities;

public class CapabilityType
{
    public string Name { get; set; } = default!;
    public string ActuatorMode { get; set; } = default!;
    public CapabilityType(string name, string mode)
    {
        Name = name;
        ActuatorMode = mode;
    }

    public CapabilityType() { }
}