using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ISettingsRepository : IRepository
{
    Task<IEnumerable<Settings>> GetAllAsync(CancellationToken cancellationToken);
}
