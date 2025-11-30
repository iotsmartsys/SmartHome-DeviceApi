using Core.Entities;
using Core.Defaults;

namespace Api.Models;

public record class Device(
    string device_id
    , string device_name
    , string description
    , string state
    , string mac_address
    , string ip_address
    , string protocol
    , string platform
    , string? last_active
    , string? power_on
    , IEnumerable<Capability> capabilities
    , IEnumerable<Property> properties
)
{
    public int? id { get; set; }
    public IDictionary<string, object> settings = new Dictionary<string, object>();

    public static implicit operator Device(Core.Entities.Device device) => new(
        device.DeviceId,
        device.Name,
        device.Description,
        device.State,
        device.MacAddress,
        device.IpAddress,
        device.Protocol.ToString(),
        device.Platform,
        device.LastActive.AsString(),
        device.PowerOn?.AsString(),
        device.Capabilities.Select(c => (Capability)c!),
        device.Properties.Select(p => (Property)p)
    )
    {
        id = device.Id,
        settings = ConvertSettingsToDictionary(device.Settings)
    };

    public static implicit operator Core.Entities.Device(Device device) => new Core.Entities.Device(
        device.device_id,
        device.device_name,
        device.state
    )
    {
        Description = device.description,
        MacAddress = device.mac_address,
        IpAddress = device.ip_address,
        Protocol = Enum.Parse<CommunicationProtocol>(device.protocol),
        Platform = device.platform,
        LastActive = device.last_active?.ToDateTime() ?? DateTime.UtcNow,
        PowerOn = device.power_on?.ToDateTime()
    }
    .AddCapabilities(device.capabilities.Select(c => (Core.Entities.Capability)c))
    .AddProperties(device.properties.Select(p => (Core.Entities.Property)p));
   

   static Dictionary<string, object> ConvertSettingsToDictionary(IEnumerable<Core.Entities.Settings> settings)
    {
        var dict = new Dictionary<string, object>();
        foreach (var setting in settings)
        {
            dict[setting.Name] = Parse(setting.Value);
        }
        return dict;
    }
   static object Parse(string value)
    {
        if (int.TryParse(value, out int intValue))
            return intValue;
        if (bool.TryParse(value, out bool boolValue))
            return boolValue;
        if (double.TryParse(value, out double doubleValue))
            return doubleValue;
        if (DateTime.TryParse(value, out DateTime dateTimeValue))
            return dateTimeValue;

        return value;
    }

}
