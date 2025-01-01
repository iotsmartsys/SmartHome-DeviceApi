namespace Api.Models;

public record class Capability(string capability_name, string type, string mode, string value)
{
    public static Capability Create(Core.Capability capability) => new Capability(capability.Name, capability.Type, capability.Mode, capability.Value);
}

public record class Device(string device_id, string device_name, string last_active, string state, IEnumerable<Capability> capabilities)
{
    public static Device Create(Core.Device device) => new Device(device.DeviceId, device.DeviceName, device.LastActive, device.State, [.. device.Capabilities.Select(Capability.Create)]);
}