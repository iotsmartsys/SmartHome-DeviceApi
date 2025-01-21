namespace Core.Entities;
public class Device
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime LastActive { get; set; } = default!;
    public string State { get; set; } = default!;
    public string MacAddress { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public CommunicationProtocol Protocol { get; set; } = CommunicationProtocol.HTTP;
    public string Platform { get; set; } = default!;
    public Device(string device_id, string device_name,  string state)
    {
        DeviceId = device_id;
        Name = device_name;
        State = state;
    }
    public Device() { }
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
}

