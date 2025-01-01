using System;

namespace Core.Contracts.Repositories;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetDevicesAsync();
}
