namespace Core.Entities;

public class CapabilityType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string ActuatorMode { get; set; } = default!;
    public string DataType { get; set; } = default!;
    public bool ComputedValue { get; set; } = default!;

    public CapabilityType() { }

    public CapabilityType(string name, string actuator_mode, string data_type, bool computed_value)
    {
        Name = name;
        ActuatorMode = actuator_mode;
        DataType = data_type;
        ComputedValue = computed_value;
    }
}