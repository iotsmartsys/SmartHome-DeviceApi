using Newtonsoft.Json;

namespace Api.Models;

[JsonConverter(typeof(DashboardRequestConverter))]
public abstract class DashboardWidgetWriteRequest
{
    private string? _title;
    private bool titleSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? title { get => _title; set { _title = value; titleSpecified = true; } }

    private string? _deviceId;
    private bool deviceIdSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? deviceId { get => _deviceId; set { _deviceId = value; deviceIdSpecified = true; } }

    private int? _capabilityId;
    private bool capabilityIdSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? capabilityId { get => _capabilityId; set { _capabilityId = value; capabilityIdSpecified = true; } }

    private string? _widgetType;
    private bool widgetTypeSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? widgetType { get => _widgetType; set { _widgetType = value; widgetTypeSpecified = true; } }

    private string? _dataMode;
    private bool dataModeSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? dataMode { get => _dataMode; set { _dataMode = value; dataModeSpecified = true; } }

    private DashboardPositionRequest? _position;
    private bool positionSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public DashboardPositionRequest? position { get => _position; set { _position = value; positionSpecified = true; } }

    private DashboardWidgetConfig? _config;
    private bool configSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public DashboardWidgetConfig? config { get => _config; set { _config = value; configSpecified = true; } }

    private int? _refreshIntervalSeconds;
    private bool refreshIntervalSecondsSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? refreshIntervalSeconds { get => _refreshIntervalSeconds; set { _refreshIntervalSeconds = value; refreshIntervalSecondsSpecified = true; } }

    private int? _displayOrder;
    private bool displayOrderSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? displayOrder { get => _displayOrder; set { _displayOrder = value; displayOrderSpecified = true; } }

    public Core.Contracts.Services.DashboardWidgetRequest ToRequest() => new()
    {
        Title = title, TitleSpecified = titleSpecified,
        DeviceId = deviceId, DeviceIdSpecified = deviceIdSpecified,
        CapabilityId = capabilityId, CapabilityIdSpecified = capabilityIdSpecified,
        WidgetType = widgetType, WidgetTypeSpecified = widgetTypeSpecified,
        DataMode = dataMode, DataModeSpecified = dataModeSpecified,
        Position = position?.ToEntity(), PositionSpecified = positionSpecified,
        Config = config?.ToEntity(), ConfigSpecified = configSpecified,
        RefreshIntervalSeconds = refreshIntervalSeconds, RefreshIntervalSecondsSpecified = refreshIntervalSecondsSpecified,
        DisplayOrder = displayOrder, DisplayOrderSpecified = displayOrderSpecified,
    };
}

public sealed class DashboardWidgetCreateRequest : DashboardWidgetWriteRequest;

public sealed class DashboardWidgetUpdateRequest : DashboardWidgetWriteRequest;
