using System;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace Core.Services;

internal class AddCapabilityService(ILogger<AddCapabilityService> logger
    , ICapabilityRepository repository) : IAddCapabilityService
{
    public async Task AddAsync(CapabilityRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adicionando novas capacidades ao dispositivo {DeviceId}", request.DeviceId);
        await repository.AddAsync(request.DeviceId, request.Capabilities);
        logger.LogInformation("Capacidades adicionadas ao dispositivo {DeviceId}", request.DeviceId);
    }
}
