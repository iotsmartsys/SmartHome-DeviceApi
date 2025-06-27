namespace Core.Entities;

public class Group
{
    readonly List<Capability> capabilitiesList = new();
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public IconGroup? Icon { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<Capability> Capabilities => capabilitiesList.AsReadOnly();

    public void AddCapability(Capability capability)
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
