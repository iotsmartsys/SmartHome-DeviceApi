using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ISettingsRepository : IRepository
{
    Task<IEnumerable<Settings>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Settings settings, CancellationToken cancellationToken);
    Task SetValueAsync(string name, string value, CancellationToken cancellationToken);
    Task SetValuesAsync(IEnumerable<Settings> settings, CancellationToken cancellationToken);
}
