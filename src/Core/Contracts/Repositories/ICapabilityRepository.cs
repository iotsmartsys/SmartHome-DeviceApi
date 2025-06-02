using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityRepository : IRepository
{
    Task<IEnumerable<Capability>> GetCapabilitiesByDeviceAsync(string device_id, CapabilityFind? capabilityQuery, CancellationToken cancellationToken);
    Task AddAsync(string device_id, IEnumerable<Capability> enumerable);
    Task DeleteAsync(string device_id, int id);
    Task UpdateAsync(string device_id, Capability capability);
    Task<IEnumerable<Capability>> GetByDeviceAndNameAsync(string device_id, CancellationToken cancellationToken, params string[] capability_name);
    Task<Capability?> GetByIdAsync(string device_id, int id, CancellationToken cancellationToken);
}
public record class CapabilityFind(string? name, string? type, string? owner, string? value,
    bool? active)
{
    public CapabilityFind() : this(null, null, null, null, null) { }
}