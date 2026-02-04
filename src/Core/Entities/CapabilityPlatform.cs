
namespace Core.Entities;

public class CapabilityPlatform
{
    public int Id { get; set; }
    public string Platform { get; set; } = default!;
    public string? ReferenceId { get; set; }
    public CapabilityPlatform(int id, string platform, string? referenceId = null)
    {
        Id = id;
        Platform = platform;
        ReferenceId = referenceId;
    }

    public CapabilityPlatform(string platform, string? referenceId = null)
    {
        Platform = platform;
        ReferenceId = referenceId;
    }
    public CapabilityPlatform() { }
}
