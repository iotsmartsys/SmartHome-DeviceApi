using System.Text.Json;
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
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCapabilities([FromQuery] CapabilityFind? capabilityQuery, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetAllCapabilitiesAsync(capabilityQuery, cancellationToken);
        if (capabilities.Any())
            return Ok(capabilities.Select(c => (Capability?)c));

        return NotFound();
    }

    [HttpGet("tiny")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CapabilityTiny>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCapabilitiesTiny([FromQuery] CapabilityFind? capabilityQuery, [FromQuery] string? format, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        string[] excludeTypes = new[] { "Message", "Alexa", "TIME_OF_DAY", "Scene" };
        var capabilities = await repository.GetAllCapabilitiesAsync(capabilityQuery, cancellationToken);
        if (capabilities.Any())
        {
            var filtereds = capabilities
               .Where(x => !excludeTypes.Contains(x.Type));
               
            if (string.Compare(format, "mcu", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var csv = string.Join("\n", filtereds.Select(c => $"{c?.Name};{c?.Value};{c?.UpdatedAt:yyyy-MM-dd HH:mm:ss}"));
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/plain", "capabilities.csv");
            }

            return Ok(filtereds.Select(c => (CapabilityTiny?)c));
        }

        return NotFound();
    }

    [HttpGet("platforms/{reference_id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityByReferenceId([FromRoute] string reference_id, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando capability {reference_id} no Cache", reference_id);

        var entity = await repository.GetByReferenceIdAsync(cancellationToken, reference_id);

        if (entity is null)
        {
            logger.LogWarning("Capability {reference_id} não encontrado", reference_id);
            return NotFound();
        }

        Capability? capability = (Capability?)entity;
        return Ok(capability);
    }

    [HttpGet("{capability_name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityByName([FromRoute] string capability_name, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando capability {capability_name} no Cache", capability_name);
        var entity = await repository.GetByNameAsync(cancellationToken, capability_name);

        if (entity is null)
        {
            logger.LogWarning("Capability {capability_name} não encontrado", capability_name);
            return NotFound();
        }

        Capability? capability = (Capability?)entity;
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
        await repository.UpdateValueAsync(capability.capability_name, capability.value, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/patches")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchAsync([FromRoute] int id, [FromBody] JsonPatchDocument<Capability> request, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return NotFound();

        var model = (Capability)entity!;

        request.ApplyTo(model);

        entity = model;
        await repository.UpdateAsync(entity, cancellationToken);
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