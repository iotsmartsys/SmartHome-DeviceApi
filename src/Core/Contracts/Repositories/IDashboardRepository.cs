using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDashboardRepository
{
    // This transaction and mutex belong only to Dashboard; reads inside the callback share it.
    Task<T> WithWriteLockAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken);
    Task<IReadOnlyList<Dashboard>> GetAllAsync(CancellationToken cancellationToken);
    Task<Dashboard?> GetAsync(long id, CancellationToken cancellationToken);
    Task SaveAsync(Dashboard dashboard, bool create, CancellationToken cancellationToken);
    Task SaveWidgetAsync(DashboardWidget widget, bool create, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
    Task DeleteWidgetAsync(long dashboardId, long widgetId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DashboardWidgetType>> GetWidgetTypesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DashboardCapabilitySource>> GetSourcesAsync(CancellationToken cancellationToken);
    Task<DashboardCapabilitySource?> GetSourceAsync(int id, CancellationToken cancellationToken);
    Task<bool> DeviceExistsAsync(string deviceId, CancellationToken cancellationToken);
}
