using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityRepository : IRepository
{
    Task<IEnumerable<Capability>> GetCapabilitiesByDeviceAsync(string device_id, CapabilityFind? capabilityQuery, CancellationToken cancellationToken);
    Task AddForDeviceAsync(string device_id, IEnumerable<Capability> enumerable);
    Task RemoveFromDeviceAsync(string device_id, IEnumerable<Capability> enumerable);
    Task UpdateForDeviceAsync(string device_id, Capability capability);
    Task<IEnumerable<Capability>> GetByDeviceAndNameAsync(string device_id, params string[] capability_name);
}

public record class CapabilityFind(string? name, string? type, string? owner, string? value)
{
    public CapabilityFind() : this(null, null, null, null) { }
}