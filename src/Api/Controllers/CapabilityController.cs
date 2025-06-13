using Api.Models;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[Route("api/v1/devices/{device_id}/capabilities")]
[Route("api/v1/capabilities")]
[ApiController]
public class CapabilityController(ILogger<CapabilityController> logger) : ControllerBase
{
    [HttpGet("by-device")]
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

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCapabilities([FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetAllCapabilitiesAsync(cancellationToken);
        if (capabilities.Any())
            return Ok(capabilities.Select(c => (Capability?)c));

        return NotFound();
    }

    [HttpGet("{capability_name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityByName([FromRoute] string capability_name, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando capability {capability_name} no Cache", capability_name);
        IEnumerable<Core.Entities.Capability> capabilities = await repository.GetByNameAsync(cancellationToken, capability_name);

        if (capabilities.Any() is false)
        {
            logger.LogWarning("Capability {capability_name} não encontrado", capability_name);
            return NotFound();
        }

        Capability? capability = (Capability?)capabilities.FirstOrDefault();
        return Ok(capability);
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
    public async Task<IActionResult> UpdateCapabilities([FromBody] CapabilityUpdate capability, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetByNameAsync(cancellationToken, capability.capability_name);
        if (capabilities.Any() is false)
            return NotFound();

        var entity = capabilities.First();
        entity.UpdateValue(capability.value);
        await repository.UpdateAsync(entity);
        return NoContent();
    }

    [HttpPatch("{id}/patches")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchAsync( [FromRoute] int id, [FromBody] JsonPatchDocument<Capability> request, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return NotFound();

        var model = (Capability)entity!;

        request.ApplyTo(model);

        entity = model;
        await repository.UpdateAsync(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCapabilities([FromRoute] int id, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(id);
        return NoContent();
    }
}