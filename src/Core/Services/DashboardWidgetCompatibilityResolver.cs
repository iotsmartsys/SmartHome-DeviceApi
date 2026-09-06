using Core.Contracts.Services;
using Core.Entities;

namespace Core.Services;

internal class DashboardWidgetCompatibilityResolver : IDashboardWidgetCompatibilityResolver
{
    public IEnumerable<DashboardWidgetType> GetCompatibleWidgets(DashboardCapability capability, IEnumerable<DashboardWidgetType> types) =>
        types.Where(type => capability.DataType is not null && type.Enabled && type.DefaultDataMode == "current_value" &&
            type.CompatibleDataTypes.Contains(capability.DataType, StringComparer.Ordinal))
            .OrderBy(type => type.Code, StringComparer.Ordinal).ToArray();
}
