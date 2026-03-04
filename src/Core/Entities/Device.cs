namespace Core.Entities;

public class Device
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime LastActive { get; set; } = default!;
    public DateTime? PowerOn { get; set; }
    public string State { get; set; } = default!;
    public string MacAddress { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public CommunicationProtocol Protocol { get; set; } = CommunicationProtocol.AMQP;
    public string Platform { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public Device() { }
    public Device(string device_id, string device_name, string state)
    {
        DeviceId = device_id;
        Name = device_name;
        State = state;
    }
    public IEnumerable<Capability> Capabilities { get; private set; } = [];
    public Capability AddCapability(Capability capability)
    {
        if (capability == null)
            return default!;

        Capability? existingCapability = Capabilities.FirstOrDefault(c => c.Name == capability.Name);
        if (existingCapability != null)
            return existingCapability!;

        Capabilities = Capabilities.Append(capability);

        return capability;
    }

    public Device AddCapabilities(IEnumerable<Capability> capabilities)
    {
        var capabilitiesNotPresentIn = capabilities.Where(c => !Capabilities.Any(c2 => c2.Id == c.Id)).ToArray();
        if (capabilitiesNotPresentIn.Length == 0)
            return this;

        Capabilities = Capabilities.Concat(capabilitiesNotPresentIn);
        return this;
    }

    public IEnumerable<Property> Properties { get; private set; } = [];

    public IEnumerable<Settings> Settings { get; private set; } = [];

    public Device AddProperty(Property property)
    {
        if (property == null || Properties.Any(p => p.Id == property.Id))
            return this;
        Properties = Properties.Append(property);
        return this;
    }

    public Device AddProperties(IEnumerable<Property> properties)
    {
        var propertiesNotPresentIn = properties.Where(p => !Properties.Any(p2 => p2.Id == p.Id)).ToArray();
        if (propertiesNotPresentIn.Length == 0)
            return this;

        Properties = Properties.Concat(propertiesNotPresentIn);
        return this;
    }

    public void ClearCapabilities()
    {
        Capabilities = [];
    }

    public void ClearProperties()
    {
        Properties = [];
    }

    public void ClearSettings()
    {
        Settings = [];
    }

    public Device AddSetting(Settings setting)
    {
        if (setting == null || Settings.Any(s => s.Id == setting.Id))
            return this;

        Settings = Settings.Append(setting);
        return this;
    }

    public Device AddSettings(IEnumerable<Settings> settings)
    {
        var settingsNotPresentIn = settings.Where(s => !Settings.Any(s2 => s2.Name == s.Name)).ToArray();
        if (settingsNotPresentIn.Length == 0)
            return this;

        Settings = Settings.Concat(settingsNotPresentIn);
        return this;
    }

    public bool HasSettingsMqtt() => Settings.Any(s => SettingsKeyTypes.mqtt_primary_broker.Is(s.Name));
}

