using Core.Contracts.Services;
using Core.Exceptions;

namespace Core.Entities;

public class DashboardWidget
{
    public long Id { get; set; }
    public long DashboardId { get; set; }
    public int CapabilityId { get; set; }
    public string? Title { get; set; }
    public string WidgetType { get; set; } = default!;
    public string DataMode { get; set; } = "current_value";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    public DashboardWidgetConfig Config { get; set; } = new();
    public bool ConfigurationInvalid { get; set; }
    public int? RefreshIntervalSeconds { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool Update(DashboardWidgetRequest request, DashboardWidgetType type, DashboardCapability source, bool create = false)
    {
        var old = (CapabilityId, Title, WidgetType, DataMode, X, Y, Width, Height, RefreshIntervalSeconds, DisplayOrder);
        var oldConfig = Config.Copy();
        if (create || request.CapabilityIdSpecified) CapabilityId = request.CapabilityId ?? 0;
        if (create || request.WidgetTypeSpecified) WidgetType = request.WidgetType!;
        DashboardValidation.Number(CapabilityId, "capabilityId", "INVALID_REQUEST", 1, int.MaxValue);
        if (!type.Enabled) throw new DashboardExceptionDomain("WIDGET_TYPE_DISABLED", "Tipo de widget desabilitado.", "widgetType");
        if (source.DataType is null) throw new DashboardExceptionDomain("UNSUPPORTED_CAPABILITY_DATA_TYPE", "Tipo de dado não suportado.", "capabilityId");
        if (!type.CompatibleDataTypes.Contains(source.DataType, StringComparer.Ordinal))
            throw new DashboardExceptionDomain("INVALID_WIDGET_FOR_CAPABILITY", "Widget incompatível com a capability.", "widgetType");
        if (request.TitleSpecified) Title = DashboardValidation.Text(request.Title, "title", "INVALID_WIDGET_TITLE", 120);
        if (create) DataMode = type.DefaultDataMode;
        if (request.DataModeSpecified) DataMode = request.DataMode ?? "current_value";
        if (DataMode != "current_value") throw DashboardValidation.Invalid("INVALID_DATA_MODE", "dataMode");
        if (request.DisplayOrderSpecified) DisplayOrder = request.DisplayOrder ?? 0;
        DashboardValidation.Number(DisplayOrder, "displayOrder", "INVALID_DISPLAY_ORDER", 0, int.MaxValue);
        if (request.RefreshIntervalSecondsSpecified) RefreshIntervalSeconds = request.RefreshIntervalSeconds;
        if (RefreshIntervalSeconds.HasValue) DashboardValidation.Number(RefreshIntervalSeconds.Value, "refreshIntervalSeconds", "INVALID_REFRESH_INTERVAL", 1, 86400);
        if (request.PositionSpecified)
        {
            if (request.Position is null) { X = Y = 0; Width = Height = 1; }
            else
            {
                if (request.Position.XSpecified) X = request.Position.X ?? 0;
                if (request.Position.YSpecified) Y = request.Position.Y ?? 0;
                if (request.Position.WidthSpecified) Width = request.Position.Width ?? 1;
                if (request.Position.HeightSpecified) Height = request.Position.Height ?? 1;
            }
        }
        DashboardValidation.Number(X, "position.x", "INVALID_WIDGET_POSITION", 0, int.MaxValue);
        DashboardValidation.Number(Y, "position.y", "INVALID_WIDGET_POSITION", 0, int.MaxValue);
        DashboardValidation.Number(Width, "position.width", "INVALID_WIDGET_POSITION", 1, 4);
        DashboardValidation.Number(Height, "position.height", "INVALID_WIDGET_POSITION", 1, 4);
        if (create || request.ConfigSpecified)
        {
            Config = type.DefaultConfig.Merge(request.Config);
            ConfigurationInvalid = false;
        }
        if (ConfigurationInvalid) throw DashboardValidation.Invalid("INVALID_WIDGET_CONFIG", "config");
        Config.Validate(WidgetType);
        var changed = old != (CapabilityId, Title, WidgetType, DataMode, X, Y, Width, Height, RefreshIntervalSeconds, DisplayOrder) || !Config.HasSameValues(oldConfig);
        if (create) CreatedAt = DashboardValidation.UtcNow();
        else if (changed) UpdatedAt = DashboardValidation.UtcNow();
        return create || changed;
    }
}
