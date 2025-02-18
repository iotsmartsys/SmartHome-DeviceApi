
namespace Incomming.Service.Core.Contracts.Facades;

public interface IDeviceFacade
{
    Task<Device?> GetDeviceAsync(string device_id);
}

public record class Device();