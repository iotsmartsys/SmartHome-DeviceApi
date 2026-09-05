using System.Text.Json.Nodes;

namespace Core.Entities;

public sealed class Dashboard
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string LayoutType { get; set; } = "grid";
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DashboardWidget> Widgets { get; set; } = [];
}

public sealed class DashboardWidget
{
    public long Id { get; set; }
    public long DashboardId { get; set; }
    public int CapabilityId { get; set; }
    public string? Title { get; set; }
    public string WidgetType { get; set; } = "";
    public string DataMode { get; set; } = "current_value";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    public string ConfigJson { get; set; } = "{}";
    public int? RefreshIntervalSeconds { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public JsonObject Config => JsonNode.Parse(ConfigJson)!.AsObject();
}

public sealed class DashboardWidgetType
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CompatibleDataTypesJson { get; set; } = "[]";
    public string DefaultDataMode { get; set; } = "current_value";
    public string DefaultConfigJson { get; set; } = "{}";
    public bool Enabled { get; set; }
    public string Lifecycle { get; set; } = "available";
    public string[] CompatibleDataTypes => System.Text.Json.JsonSerializer.Deserialize<string[]>(CompatibleDataTypesJson)!;
    public JsonObject DefaultConfig => JsonNode.Parse(DefaultConfigJson)!.AsObject();
}

// Dashboard-specific projection: LEFT JOINs retain capabilities whose related rows disappeared.
public sealed class DashboardCapabilitySource
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
}

public sealed record DashboardReading(object? Value, string? Unit, string? Label,
    string? Icon, string Status, DateTimeOffset? LastUpdatedAt);
