using Api.Models;
using Core.Contracts.Events;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[Route("api/v1/devices/{device_id}/capabilities")]
[ApiController]
public class CapabilityController(ILogger<CapabilityController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilities([FromRoute] string device_id, [FromQuery] CapabilityFind? capabilityQuery, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetCapabilitiesByDeviceAsync(device_id, capabilityQuery, cancellationToken);
        if (capabilities.Any())
            return Ok(capabilities.Select(c => (Capability?)c));

        return NotFound();
    }

    [HttpGet("{capability_name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityByName([FromRoute] string device_id, [FromRoute] string capability_name, [FromServices] IMemoryCache cache, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando capability {capability_name} para o device {device_id} no Cache", capability_name, device_id);
        Capability? capability = await cache.GetOrCreateAsync($"{device_id}-{capability_name}", async entry =>
        {
            logger.LogInformation("Buscando capability {capability_name} para o device {device_id} no Banco de Dados", capability_name, device_id);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
            IEnumerable<Core.Entities.Capability> capabilities = await repository.GetByDeviceAndNameAsync(device_id, cancellationToken, capability_name);
            return (Capability?)capabilities.FirstOrDefault();
        });

        if (capability is not null)
            return Ok(capability);

        logger.LogWarning("Capability {capability_name} não encontrado para o device {device_id}", capability_name, device_id);
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCapabilitiesAsync([FromRoute] string device_id, [FromBody] IEnumerable<Capability> capabilities, [FromServices] IAddCapabilityService service, CancellationToken cancellationToken)
    {
        await service.AddAsync(new(device_id, capabilities.Select(c => (Core.Entities.Capability)c)), cancellationToken);
        return NoContent();
    }

    [HttpPatch()]
    [HttpPatch("value")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCapabilities([FromRoute] string device_id, [FromBody] CapabilityUpdate capability, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetByDeviceAndNameAsync(device_id, cancellationToken, capability.capability_name);
        if (capabilities.Any() is false)
            return NotFound();

        var entity = capabilities.First();
        entity.UpdateValue(capability.value);
        await repository.UpdateAsync(device_id, entity);
        return NoContent();
    }

    [HttpPatch("patches/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchAsync([FromRoute] string device_id, [FromRoute] int id, [FromBody] JsonPatchDocument<Capability> request, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(device_id, id, cancellationToken);
        if (entity is null)
            return NotFound();

        var model = (Capability)entity!;

        request.ApplyTo(model);

        entity = model;
        await repository.UpdateAsync(device_id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCapabilities([FromRoute] string device_id, [FromRoute] int id, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(device_id, id);
        return NoContent();
    }

    [HttpPut("{capability_name}/state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateState([FromRoute] string device_id, [FromRoute] string capability_name, [FromBody] CapabilityStateRequest state, [FromServices] ICapabilityRepository repository, [FromServices] IEventPublisher publisher, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetByDeviceAndNameAsync(device_id, cancellationToken, capability_name);
        if (capabilities.Any() is false)
            return NotFound();

        var entity = capabilities.First();
        if (entity.Owner is null)
            return NoContent();

        string queue = $"device.{entity.Owner}.command";
        await publisher.PublishAsync(queue, new CapabilityCommand(entity.Owner, entity.Name, state.state), cancellationToken);
        return NoContent();
    }
    public record class CapabilityStateRequest(string state);
}