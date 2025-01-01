namespace Core;

public class Capability
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Mode { get; set; } = default!;
    public string Value { get; set; } = default!;
    public Capability(string name, string type, string mode, string value)
    {
        Name = name;
        Type = type;
        Mode = mode;
        Value = value;
    }

    public Capability() { }

}
public class Device
{
    public string DeviceId { get; set; } = default!;
    public string DeviceName { get; set; } = default!;
    public string LastActive { get; set; } = default!;
    public string State { get; set; } = default!;
    public Device(string device_id, string device_name, string last_active, string state)
    {
        DeviceId = device_id;
        DeviceName = device_name;
        LastActive = last_active;
        State = state;
    }
    public Device() { }
    public IEnumerable<Capability> Capabilities { get; private set; } = [];
    public Device AddCapability(Capability capability)
    {
        Capabilities = Capabilities.Append(capability);
        return this;
    }
}