using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDeviceCapabilityRepository : IRepository
{
    Task<IEnumerable<Capability>> GetCapabilitiesByDeviceAsync(string device_id);
    Task AddForDeviceAsync(string device_id, IEnumerable<Capability> enumerable);
    Task RemoveFromDeviceAsync(string device_id, IEnumerable<Capability> enumerable);
    Task UpdateForDeviceAsync(string device_id, Capability capability);
    Task<IEnumerable<Capability>> GetByDeviceAndNameAsync(string device_id, params string[] capability_name);
}
