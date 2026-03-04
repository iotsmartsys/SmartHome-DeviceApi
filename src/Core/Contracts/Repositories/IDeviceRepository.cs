using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IDeviceRepository : IRepository
{
    Task CreateAsync(Device entity, CancellationToken cancellationToken);
    Task<Device?> GetDeviceAsync(string device_id, CancellationToken cancellationToken);
    Task<IEnumerable<Device>> GetDevicesAsync(DeviceFind? find, CancellationToken cancellationToken);
    Task UpdateAsync(Device device, CancellationToken cancellationToken);
    Task DeleteAsync(string device_id, CancellationToken cancellationToken);
}
public record class DeviceFind()
{
    public string? DeviceId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Platform { get; init; }
    public bool? IsActive { get; init; }
};