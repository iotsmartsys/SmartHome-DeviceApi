using Core.Entities;

namespace Core.Contracts.Services;

public interface IDashboardWidgetCompatibilityResolver
{
    IEnumerable<DashboardWidgetType> GetCompatibleWidgets(DashboardCapability capability, IEnumerable<DashboardWidgetType> types);
}
