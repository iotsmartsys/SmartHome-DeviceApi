using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDeviceMetricsRepository : IRepository
{
    Task<bool> SaveAsync(string externalDeviceId, DeviceMetrics metrics, CancellationToken cancellationToken);
    Task<DeviceMetricsCurrent?> GetCurrentAsync(string externalDeviceId, CancellationToken cancellationToken);
    Task<bool> DeviceExistsAsync(string externalDeviceId, CancellationToken cancellationToken);
    Task<DeviceMetricsHistoryPage> GetHistoryAsync(
        string externalDeviceId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
