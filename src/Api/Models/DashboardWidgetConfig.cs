using Core.Entities;
using Newtonsoft.Json;

namespace Api.Models;

[JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
[JsonConverter(typeof(DashboardRequestConverter))]
public sealed class DashboardWidgetConfig
{
    private string? _unit;
    private bool unitSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? unit { get => _unit; set { _unit = value; unitSpecified = true; } }

    private double? _min;
    private bool minSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public double? min { get => _min; set { _min = value; minSpecified = true; } }

    private double? _max;
    private bool maxSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public double? max { get => _max; set { _max = value; maxSpecified = true; } }

    private double? _warningFrom;
    private bool warningFromSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public double? warningFrom { get => _warningFrom; set { _warningFrom = value; warningFromSpecified = true; } }

    private double? _dangerFrom;
    private bool dangerFromSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public double? dangerFrom { get => _dangerFrom; set { _dangerFrom = value; dangerFromSpecified = true; } }

    private int? _decimals;
    private bool decimalsSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? decimals { get => _decimals; set { _decimals = value; decimalsSpecified = true; } }

    private bool? _showLastUpdated;
    private bool showLastUpdatedSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public bool? showLastUpdated { get => _showLastUpdated; set { _showLastUpdated = value; showLastUpdatedSpecified = true; } }

    private bool? _invertState;
    private bool invertStateSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public bool? invertState { get => _invertState; set { _invertState = value; invertStateSpecified = true; } }

    private string? _onLabel;
    private bool onLabelSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? onLabel { get => _onLabel; set { _onLabel = value; onLabelSpecified = true; } }

    private string? _onIcon;
    private bool onIconSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? onIcon { get => _onIcon; set { _onIcon = value; onIconSpecified = true; } }

    private string? _offLabel;
    private bool offLabelSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? offLabel { get => _offLabel; set { _offLabel = value; offLabelSpecified = true; } }

    private string? _offIcon;
    private bool offIconSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? offIcon { get => _offIcon; set { _offIcon = value; offIconSpecified = true; } }

    private string? _openLabel;
    private bool openLabelSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? openLabel { get => _openLabel; set { _openLabel = value; openLabelSpecified = true; } }

    private string? _openIcon;
    private bool openIconSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? openIcon { get => _openIcon; set { _openIcon = value; openIconSpecified = true; } }

    private string? _closedLabel;
    private bool closedLabelSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? closedLabel { get => _closedLabel; set { _closedLabel = value; closedLabelSpecified = true; } }

    private string? _closedIcon;
    private bool closedIconSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? closedIcon { get => _closedIcon; set { _closedIcon = value; closedIconSpecified = true; } }

    private string? _pressedLabel;
    private bool pressedLabelSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? pressedLabel { get => _pressedLabel; set { _pressedLabel = value; pressedLabelSpecified = true; } }

    private string? _pressedIcon;
    private bool pressedIconSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? pressedIcon { get => _pressedIcon; set { _pressedIcon = value; pressedIconSpecified = true; } }

    private string? _releasedLabel;
    private bool releasedLabelSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? releasedLabel { get => _releasedLabel; set { _releasedLabel = value; releasedLabelSpecified = true; } }

    private string? _releasedIcon;
    private bool releasedIconSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? releasedIcon { get => _releasedIcon; set { _releasedIcon = value; releasedIconSpecified = true; } }

    public bool ShouldSerializeunit() => unitSpecified;
    public bool ShouldSerializemin() => minSpecified;
    public bool ShouldSerializemax() => maxSpecified;
    public bool ShouldSerializewarningFrom() => warningFromSpecified;
    public bool ShouldSerializedangerFrom() => dangerFromSpecified;
    public bool ShouldSerializedecimals() => decimalsSpecified;
    public bool ShouldSerializeshowLastUpdated() => showLastUpdatedSpecified;
    public bool ShouldSerializeinvertState() => invertStateSpecified;
    public bool ShouldSerializeonLabel() => onLabelSpecified;
    public bool ShouldSerializeonIcon() => onIconSpecified;
    public bool ShouldSerializeoffLabel() => offLabelSpecified;
    public bool ShouldSerializeoffIcon() => offIconSpecified;
    public bool ShouldSerializeopenLabel() => openLabelSpecified;
    public bool ShouldSerializeopenIcon() => openIconSpecified;
    public bool ShouldSerializeclosedLabel() => closedLabelSpecified;
    public bool ShouldSerializeclosedIcon() => closedIconSpecified;
    public bool ShouldSerializepressedLabel() => pressedLabelSpecified;
    public bool ShouldSerializepressedIcon() => pressedIconSpecified;
    public bool ShouldSerializereleasedLabel() => releasedLabelSpecified;
    public bool ShouldSerializereleasedIcon() => releasedIconSpecified;

    public Core.Entities.DashboardWidgetConfig ToEntity()
    {
        Core.Entities.DashboardWidgetConfig entity = new();
        if (unitSpecified) entity.SetValue(DashboardConfigField.Unit, unit);
        if (minSpecified) entity.SetValue(DashboardConfigField.Min, min);
        if (maxSpecified) entity.SetValue(DashboardConfigField.Max, max);
        if (warningFromSpecified) entity.SetValue(DashboardConfigField.WarningFrom, warningFrom);
        if (dangerFromSpecified) entity.SetValue(DashboardConfigField.DangerFrom, dangerFrom);
        if (decimalsSpecified) entity.SetValue(DashboardConfigField.Decimals, decimals);
        if (showLastUpdatedSpecified) entity.SetValue(DashboardConfigField.ShowLastUpdated, showLastUpdated);
        if (invertStateSpecified) entity.SetValue(DashboardConfigField.InvertState, invertState);
        if (onLabelSpecified) entity.SetValue(DashboardConfigField.OnLabel, onLabel);
        if (onIconSpecified) entity.SetValue(DashboardConfigField.OnIcon, onIcon);
        if (offLabelSpecified) entity.SetValue(DashboardConfigField.OffLabel, offLabel);
        if (offIconSpecified) entity.SetValue(DashboardConfigField.OffIcon, offIcon);
        if (openLabelSpecified) entity.SetValue(DashboardConfigField.OpenLabel, openLabel);
        if (openIconSpecified) entity.SetValue(DashboardConfigField.OpenIcon, openIcon);
        if (closedLabelSpecified) entity.SetValue(DashboardConfigField.ClosedLabel, closedLabel);
        if (closedIconSpecified) entity.SetValue(DashboardConfigField.ClosedIcon, closedIcon);
        if (pressedLabelSpecified) entity.SetValue(DashboardConfigField.PressedLabel, pressedLabel);
        if (pressedIconSpecified) entity.SetValue(DashboardConfigField.PressedIcon, pressedIcon);
        if (releasedLabelSpecified) entity.SetValue(DashboardConfigField.ReleasedLabel, releasedLabel);
        if (releasedIconSpecified) entity.SetValue(DashboardConfigField.ReleasedIcon, releasedIcon);
        return entity;
    }

    public static implicit operator DashboardWidgetConfig(Core.Entities.DashboardWidgetConfig entity)
    {
        DashboardWidgetConfig model = new();
        if (entity.Fields.Contains(DashboardConfigField.Unit)) model.unit = entity.Unit;
        if (entity.Fields.Contains(DashboardConfigField.Min)) model.min = entity.Min;
        if (entity.Fields.Contains(DashboardConfigField.Max)) model.max = entity.Max;
        if (entity.Fields.Contains(DashboardConfigField.WarningFrom)) model.warningFrom = entity.WarningFrom;
        if (entity.Fields.Contains(DashboardConfigField.DangerFrom)) model.dangerFrom = entity.DangerFrom;
        if (entity.Fields.Contains(DashboardConfigField.Decimals)) model.decimals = entity.Decimals;
        if (entity.Fields.Contains(DashboardConfigField.ShowLastUpdated)) model.showLastUpdated = entity.ShowLastUpdated;
        if (entity.Fields.Contains(DashboardConfigField.InvertState)) model.invertState = entity.InvertState;
        if (entity.Fields.Contains(DashboardConfigField.OnLabel)) model.onLabel = entity.OnLabel;
        if (entity.Fields.Contains(DashboardConfigField.OnIcon)) model.onIcon = entity.OnIcon;
        if (entity.Fields.Contains(DashboardConfigField.OffLabel)) model.offLabel = entity.OffLabel;
        if (entity.Fields.Contains(DashboardConfigField.OffIcon)) model.offIcon = entity.OffIcon;
        if (entity.Fields.Contains(DashboardConfigField.OpenLabel)) model.openLabel = entity.OpenLabel;
        if (entity.Fields.Contains(DashboardConfigField.OpenIcon)) model.openIcon = entity.OpenIcon;
        if (entity.Fields.Contains(DashboardConfigField.ClosedLabel)) model.closedLabel = entity.ClosedLabel;
        if (entity.Fields.Contains(DashboardConfigField.ClosedIcon)) model.closedIcon = entity.ClosedIcon;
        if (entity.Fields.Contains(DashboardConfigField.PressedLabel)) model.pressedLabel = entity.PressedLabel;
        if (entity.Fields.Contains(DashboardConfigField.PressedIcon)) model.pressedIcon = entity.PressedIcon;
        if (entity.Fields.Contains(DashboardConfigField.ReleasedLabel)) model.releasedLabel = entity.ReleasedLabel;
        if (entity.Fields.Contains(DashboardConfigField.ReleasedIcon)) model.releasedIcon = entity.ReleasedIcon;
        return model;
    }
}
