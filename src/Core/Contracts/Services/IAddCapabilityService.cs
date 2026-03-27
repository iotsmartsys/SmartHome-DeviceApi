
using Core.Entities;

namespace Core.Contracts.Services;

public record class CapabilityRequest(string DeviceId, Capability Capability);
public interface IAddCapabilityService
{
    Task AddAsync(CapabilityRequest request, CancellationToken cancellationToken);
}