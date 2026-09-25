namespace Core.Entities;

public class DashboardCapability
{
    public int CapabilityId { get; set; }
    public string? CapabilityName { get; set; }
    public string? CapabilityCode { get; set; }
    public string? SourceDataType { get; set; }
    public string? Unit { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public bool DeviceExists { get; set; }
    public bool DeviceActive { get; set; }
    public string? DeviceState { get; set; }
    public bool Active { get; set; }
    public string? Value { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string NormalizedSourceType => SourceDataType?.Trim().ToLowerInvariant() ?? "";
    public string? DataType => NormalizedSourceType switch
    {
        "float" or "integer" => "numeric", "boolean" or "detection" => "logical",
        "open_closed" or "on_off" or "power" or "press" => "state", "text" => "text", "time" => "event", _ => null
    };
    public string? SemanticType => NormalizedSourceType switch
    {
        "open_closed" => "open_closed", "on_off" or "power" => "on_off",
        "detection" => "detection", "press" => "press", "time" => "time", _ => null
    };
}

public record class DashboardReading(object? Value, string? Unit, string? Label,
    string? Icon, string Status, DateTimeOffset? LastUpdatedAt);
