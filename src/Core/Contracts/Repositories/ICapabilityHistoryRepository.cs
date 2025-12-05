namespace Core.Contracts.Repositories;

public interface ICapabilityHistoryRepository
{
    Task<IEnumerable<CapabilityHistory>> GetByCapabilityIdAsync(int capabilityId, CapabilityHistoryFind? historyFind, CancellationToken cancellationToken);
    Task AddAsync(string capability_name, string value, CancellationToken cancellationToken);
}
 
