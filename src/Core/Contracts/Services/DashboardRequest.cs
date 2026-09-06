using Core.Entities;

namespace Core.Contracts.Services;

public class DashboardRequest
{
    public string? Name { get; set; }
    public bool NameSpecified { get; set; }
    public string? Description { get; set; }
    public bool DescriptionSpecified { get; set; }
    public string? LayoutType { get; set; }
    public bool LayoutTypeSpecified { get; set; }
    public bool? IsDefault { get; set; }
    public bool IsDefaultSpecified { get; set; }
    public int? DisplayOrder { get; set; }
    public bool DisplayOrderSpecified { get; set; }
}

public class DashboardWidgetRequest
{
    public string? Title { get; set; }
    public bool TitleSpecified { get; set; }
    public string? DeviceId { get; set; }
    public bool DeviceIdSpecified { get; set; }
    public int? CapabilityId { get; set; }
    public bool CapabilityIdSpecified { get; set; }
    public string? WidgetType { get; set; }
    public bool WidgetTypeSpecified { get; set; }
    public string? DataMode { get; set; }
    public bool DataModeSpecified { get; set; }
    public DashboardPositionRequest? Position { get; set; }
    public bool PositionSpecified { get; set; }
    public DashboardWidgetConfig? Config { get; set; }
    public bool ConfigSpecified { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
    public bool RefreshIntervalSecondsSpecified { get; set; }
    public int? DisplayOrder { get; set; }
    public bool DisplayOrderSpecified { get; set; }
}

public class DashboardPositionRequest
{
    public int? X { get; set; }
    public bool XSpecified { get; set; }
    public int? Y { get; set; }
    public bool YSpecified { get; set; }
    public int? Width { get; set; }
    public bool WidthSpecified { get; set; }
    public int? Height { get; set; }
    public bool HeightSpecified { get; set; }
}
