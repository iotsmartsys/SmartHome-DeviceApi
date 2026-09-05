using Core.Entities;
using Core.Exceptions;

namespace Core.Services;

public sealed class DashboardWidgetCompatibilityResolver
{
    public static string SourceType(DashboardCapabilitySource source) => source.SourceDataType?.Trim().ToLowerInvariant() ?? "";
    public static string? VisualType(DashboardCapabilitySource? source) => source is null ? null : SourceType(source) switch
    {
        "float" or "integer" => "numeric",
        "boolean" or "detection" => "logical",
        "open_closed" or "on_off" or "power" or "press" => "state",
        "text" => "text",
        "time" => "event",
        _ => null
    };
    public static string? SemanticType(DashboardCapabilitySource source) => SourceType(source) switch
    {
        "open_closed" => "open_closed", "on_off" or "power" => "on_off",
        "detection" => "detection", "press" => "press", "time" => "time", _ => null
    };
    public IReadOnlyList<DashboardWidgetType> Compatible(DashboardCapabilitySource source, IEnumerable<DashboardWidgetType> types) =>
        types.Where(t => t.Enabled && t.DefaultDataMode == "current_value" &&
            t.CompatibleDataTypes.Contains(VisualType(source), StringComparer.Ordinal))
            .OrderBy(t => t.Code, StringComparer.Ordinal).ToArray();

    public void Validate(DashboardCapabilitySource source, DashboardWidgetType type)
    {
        if (!type.Enabled) throw new DashboardException("WIDGET_TYPE_DISABLED", 422, "Tipo de widget desabilitado.", "widgetType");
        if (VisualType(source) is not { } visual)
            throw new DashboardException("UNSUPPORTED_CAPABILITY_DATA_TYPE", 422, "Tipo de dado não suportado.", "capabilityId");
        if (!type.CompatibleDataTypes.Contains(visual, StringComparer.Ordinal))
            throw new DashboardException("INVALID_WIDGET_FOR_CAPABILITY", 422, "Widget incompatível com a capability.", "widgetType");
    }
}
