
using Core.Entities;

namespace Core.Contracts.Services;

public record class CapabilityRequest(string DeviceId, IEnumerable<Capability> Capabilities);
public interface IAddCapabilityService
{
    Task AddAsync(CapabilityRequest request, CancellationToken cancellationToken);
}