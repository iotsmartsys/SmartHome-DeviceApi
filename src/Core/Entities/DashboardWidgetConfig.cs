namespace Core.Entities;

public enum DashboardConfigField
{
    Unit, Min, Max, WarningFrom, DangerFrom, Decimals, ShowLastUpdated, InvertState, OnLabel, OffLabel, OnIcon, OffIcon, OpenLabel, ClosedLabel, OpenIcon, ClosedIcon, PressedLabel, ReleasedLabel, PressedIcon, ReleasedIcon
}

// A closed configuration contract; Fields records presence, not arbitrary JSON properties.
public class DashboardWidgetConfig
{
    public HashSet<DashboardConfigField> Fields { get; set; } = [];
    public string? Unit { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? WarningFrom { get; set; }
    public double? DangerFrom { get; set; }
    public int? Decimals { get; set; }
    public bool? ShowLastUpdated { get; set; }
    public bool? InvertState { get; set; }
    public string? OnLabel { get; set; }
    public string? OffLabel { get; set; }
    public string? OnIcon { get; set; }
    public string? OffIcon { get; set; }
    public string? OpenLabel { get; set; }
    public string? ClosedLabel { get; set; }
    public string? OpenIcon { get; set; }
    public string? ClosedIcon { get; set; }
    public string? PressedLabel { get; set; }
    public string? ReleasedLabel { get; set; }
    public string? PressedIcon { get; set; }
    public string? ReleasedIcon { get; set; }

    public object? GetValue(DashboardConfigField field) => field switch
    {
        DashboardConfigField.Unit => Unit,
        DashboardConfigField.Min => Min,
        DashboardConfigField.Max => Max,
        DashboardConfigField.WarningFrom => WarningFrom,
        DashboardConfigField.DangerFrom => DangerFrom,
        DashboardConfigField.Decimals => Decimals,
        DashboardConfigField.ShowLastUpdated => ShowLastUpdated,
        DashboardConfigField.InvertState => InvertState,
        DashboardConfigField.OnLabel => OnLabel,
        DashboardConfigField.OffLabel => OffLabel,
        DashboardConfigField.OnIcon => OnIcon,
        DashboardConfigField.OffIcon => OffIcon,
        DashboardConfigField.OpenLabel => OpenLabel,
        DashboardConfigField.ClosedLabel => ClosedLabel,
        DashboardConfigField.OpenIcon => OpenIcon,
        DashboardConfigField.ClosedIcon => ClosedIcon,
        DashboardConfigField.PressedLabel => PressedLabel,
        DashboardConfigField.ReleasedLabel => ReleasedLabel,
        DashboardConfigField.PressedIcon => PressedIcon,
        DashboardConfigField.ReleasedIcon => ReleasedIcon,
        _ => throw new ArgumentOutOfRangeException(nameof(field))
    };

    public void SetValue(DashboardConfigField field, object? value)
    {
        switch (field)
        {
            case DashboardConfigField.Unit: Unit = (string?)value; break;
            case DashboardConfigField.Min: Min = (double?)value; break;
            case DashboardConfigField.Max: Max = (double?)value; break;
            case DashboardConfigField.WarningFrom: WarningFrom = (double?)value; break;
            case DashboardConfigField.DangerFrom: DangerFrom = (double?)value; break;
            case DashboardConfigField.Decimals: Decimals = (int?)value; break;
            case DashboardConfigField.ShowLastUpdated: ShowLastUpdated = (bool?)value; break;
            case DashboardConfigField.InvertState: InvertState = (bool?)value; break;
            case DashboardConfigField.OnLabel: OnLabel = (string?)value; break;
            case DashboardConfigField.OffLabel: OffLabel = (string?)value; break;
            case DashboardConfigField.OnIcon: OnIcon = (string?)value; break;
            case DashboardConfigField.OffIcon: OffIcon = (string?)value; break;
            case DashboardConfigField.OpenLabel: OpenLabel = (string?)value; break;
            case DashboardConfigField.ClosedLabel: ClosedLabel = (string?)value; break;
            case DashboardConfigField.OpenIcon: OpenIcon = (string?)value; break;
            case DashboardConfigField.ClosedIcon: ClosedIcon = (string?)value; break;
            case DashboardConfigField.PressedLabel: PressedLabel = (string?)value; break;
            case DashboardConfigField.ReleasedLabel: ReleasedLabel = (string?)value; break;
            case DashboardConfigField.PressedIcon: PressedIcon = (string?)value; break;
            case DashboardConfigField.ReleasedIcon: ReleasedIcon = (string?)value; break;
            default: throw new ArgumentOutOfRangeException(nameof(field));
        }
        Fields.Add(field);
    }

    public DashboardWidgetConfig Copy()
    {
        var copy = new DashboardWidgetConfig();
        foreach (var field in Fields) copy.SetValue(field, GetValue(field));
        return copy;
    }

    public bool HasSameValues(DashboardWidgetConfig other) =>
        Fields.SetEquals(other.Fields) && Fields.All(field => Equals(GetValue(field), other.GetValue(field)));

    public DashboardWidgetConfig Merge(DashboardWidgetConfig? supplied)
    {
        var result = Copy();
        if (supplied is null) return result;
        foreach (var field in supplied.Fields)
        {
            if (!Fields.Contains(field)) throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config." + char.ToLowerInvariant(field.ToString()[0]) + field.ToString()[1..]);
            if (supplied.GetValue(field) is { } value) result.SetValue(field, value);
        }
        return result;
    }

    public void Validate(string widgetType)
    {
        DashboardConfigField[] allowed = widgetType switch
        {
            "value_card" => [DashboardConfigField.Unit, DashboardConfigField.Decimals, DashboardConfigField.ShowLastUpdated],
            "gauge" => [DashboardConfigField.Unit, DashboardConfigField.Min, DashboardConfigField.Max, DashboardConfigField.WarningFrom, DashboardConfigField.DangerFrom, DashboardConfigField.Decimals],
            "state_icon" or "status_card" => [DashboardConfigField.Unit, DashboardConfigField.InvertState,
                DashboardConfigField.OnLabel, DashboardConfigField.OffLabel, DashboardConfigField.OnIcon, DashboardConfigField.OffIcon,
                DashboardConfigField.OpenLabel, DashboardConfigField.ClosedLabel, DashboardConfigField.OpenIcon, DashboardConfigField.ClosedIcon,
                DashboardConfigField.PressedLabel, DashboardConfigField.ReleasedLabel, DashboardConfigField.PressedIcon, DashboardConfigField.ReleasedIcon],
            _ => throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config")
        };
        if (!Fields.SetEquals(allowed)) throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config");
        Unit = DashboardValidation.Text(Unit, "config.unit", "INVALID_WIDGET_CONFIG", 32);
        if (Fields.Contains(DashboardConfigField.Decimals) && (Decimals is null or < 0 or > 6))
            throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config.decimals");
        if (Fields.Contains(DashboardConfigField.ShowLastUpdated) && !ShowLastUpdated.HasValue ||
            Fields.Contains(DashboardConfigField.InvertState) && !InvertState.HasValue)
            throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config");
        foreach (var field in Fields.ToArray())
        {
            var name = field.ToString();
            if (name.EndsWith("Label", StringComparison.Ordinal) || name.EndsWith("Icon", StringComparison.Ordinal))
                SetValue(field, DashboardValidation.Text((string?)GetValue(field), "config." + char.ToLowerInvariant(name[0]) + name[1..],
                    "INVALID_WIDGET_CONFIG", 120, name.EndsWith("Label", StringComparison.Ordinal)));
        }
        if (widgetType != "gauge") return;
        if (!Min.HasValue || !Max.HasValue || !double.IsFinite(Min.Value) || !double.IsFinite(Max.Value) || Min >= Max)
            throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config.min");
        if (WarningFrom.HasValue && (!double.IsFinite(WarningFrom.Value) || WarningFrom < Min || WarningFrom > Max))
            throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config.warningFrom");
        if (DangerFrom.HasValue && (!double.IsFinite(DangerFrom.Value) || DangerFrom < Min || DangerFrom > Max))
            throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config.dangerFrom");
        if (WarningFrom > DangerFrom) throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config.warningFrom");
    }
}
