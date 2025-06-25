namespace Core.Entities;

public class Group
{
    readonly List<CapabilityGroup> capabilitiesList = new();
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public IconGroup? Icon { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<CapabilityGroup> Capabilities => capabilitiesList.AsReadOnly();

    public void AddCapability(CapabilityGroup capability)
    {
        if (!capabilitiesList.Any(c => c.Id == capability.Id))
            capabilitiesList.Add(capability);

    }

}
public class IconGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}
