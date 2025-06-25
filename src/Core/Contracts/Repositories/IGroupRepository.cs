using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IGroupRepository : IRepository
{
    Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken);
    Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Group group, CancellationToken cancellationToken);
    Task UpdateAsync(Group group, CancellationToken cancellationToken);
    Task UpdateOnlyGroupAsync(Group group, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task AddCapabilityToGroupAsync(int groupId, CapabilityGroup capability, CancellationToken cancellationToken);
    Task DeleteCapabilityForGroupAsync(int groupId, int capabilityId, CancellationToken cancellationToken);
}