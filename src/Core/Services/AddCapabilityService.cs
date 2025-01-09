using System;
using Core.Contracts.Events;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace Core.Services;

internal class AddCapabilityService(ILogger<AddCapabilityService> logger
    , IDeviceCapabilityRepository repository
    , IEventPublisher publisher) : IAddCapabilityService
{
    public async Task AddAsync(CapabilityRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adicionando novas capacidades ao dispositivo {DeviceId}", request.DeviceId);
        await repository.AddForDeviceAsync(request.DeviceId, request.Capabilities);
        logger.LogInformation("Capacidades adicionadas ao dispositivo {DeviceId}", request.DeviceId);

        var capabilitiesCompleted = await repository.GetByDeviceAndNameAsync(request.DeviceId, [.. request.Capabilities.Select(c => c.Name)]);

        await publisher.PublishAsync(new CapabilityRegisterEvent(action : "add",context : "capability",capabilities :[.. capabilitiesCompleted ]), cancellationToken);
    }
}
