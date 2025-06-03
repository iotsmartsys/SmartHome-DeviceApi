using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDeviceRepository : IRepository
{
    Task CreateAsync(Device entity, CancellationToken cancellationToken);
    Task<Device?> GetDeviceAsync(string device_id, CancellationToken cancellationToken);
    Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Device device, CancellationToken cancellationToken);
}