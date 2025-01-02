using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDeviceRepository : IRepository
{
    Task CreateAsync(Device entity);
    Task<Device?> GetDeviceAsync(string device_id);
    Task<IEnumerable<Device>> GetDevicesAsync();
}
