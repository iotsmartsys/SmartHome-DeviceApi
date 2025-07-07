using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityRepository : IRepository
{
    Task<IEnumerable<Capability>> GetAllCapabilitiesAsync(CapabilityFind? capabilityFind, CancellationToken cancellationToken);
    Task AddAsync(string device_id, IEnumerable<Capability> enumerable);
    Task DeleteAsync(int id);
    Task UpdateAsync(Capability capability);
    Task<Capability?> GetByNameAsync(CancellationToken cancellationToken, string capability_name);
    Task<Capability?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task UpdateValueAsync(string capability_name, string value, CancellationToken cancellationToken);
}
public record class CapabilityFind(string? name, string? type, string? owner, string? value,
    bool? active)
{
    public CapabilityFind() : this(null, null, null, null, null) { }
}