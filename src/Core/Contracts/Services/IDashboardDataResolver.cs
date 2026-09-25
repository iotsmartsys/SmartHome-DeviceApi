using Core.Entities;

namespace Core.Contracts.Services;

public interface IDashboardDataResolver
{
    DashboardReading Resolve(DashboardCapability? capability, DashboardWidget? widget,
        IEnumerable<DashboardWidgetType> types, DateTimeOffset generatedAt);
}
