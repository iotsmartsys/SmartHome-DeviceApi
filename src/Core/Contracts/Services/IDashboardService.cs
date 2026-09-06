using Core.Entities;

namespace Core.Contracts.Services;

public interface IDashboardService
{
    Task<IEnumerable<Dashboard>> GetAllAsync(CancellationToken cancellationToken);
    Task<Dashboard> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Dashboard> AddAsync(DashboardRequest request, CancellationToken cancellationToken);
    Task<Dashboard> UpdateAsync(long id, DashboardRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
    Task<DashboardWidget> AddWidgetAsync(long dashboardId, DashboardWidgetRequest request, CancellationToken cancellationToken);
    Task<DashboardWidget> UpdateWidgetAsync(long dashboardId, long widgetId, DashboardWidgetRequest request, CancellationToken cancellationToken);
    Task DeleteWidgetAsync(long dashboardId, long widgetId, CancellationToken cancellationToken);
    Task<IEnumerable<DashboardWidgetType>> GetWidgetTypesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DashboardCapability>> GetCapabilitiesAsync(CancellationToken cancellationToken);
    Task<DashboardCapability> GetCapabilityByIdAsync(int id, CancellationToken cancellationToken);
}
