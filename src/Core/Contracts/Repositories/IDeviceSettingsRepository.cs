using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDeviceSettingsRepository : IRepository
{
    Task SaveAsync(string deviceId, IEnumerable<Settings> settings, CancellationToken cancellationToken);
    Task<IEnumerable<Settings>> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken);
}
