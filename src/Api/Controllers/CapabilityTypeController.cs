using Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/capabilities-types")]
public class CapabilityTypeController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CapabilityType>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilitiesTypes([FromQuery] string? name, [FromServices] ICapabilityTypeRepository repository)
    {
        var capabilities = await repository.GetAllAsync(name);
        if (!capabilities.Any())
            return NoContent();

        return Ok(capabilities.Select(c => (CapabilityType)c));
    }

    [HttpGet("{name}", Name = "GetCapabilityType")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CapabilityType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityType([FromServices] ICapabilityTypeRepository repository, string name)
    {
        var capability = await repository.GetByNameAsync(name);
        if (capability is null)
            return NotFound();

        return Ok((CapabilityType)capability);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CapabilityType))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCapabilityType([FromBody] CapabilityType capabilityType, [FromServices] ICapabilityTypeRepository repository)
    {
        if (capabilityType is null)
            return BadRequest();

        await repository.CreateAsync((Core.Entities.CapabilityType)capabilityType);
        return CreatedAtRoute("GetCapabilityType", new { name = capabilityType.name }, capabilityType);
    }

    [HttpPatch("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCapabilityType([FromRoute] string name, [FromBody] JsonPatchDocument<CapabilityType> patch, [FromServices] ICapabilityTypeRepository repository)
    {
        var existing = await repository.GetByNameAsync(name);
        if (existing is null)
            return NotFound();

        var model = (CapabilityType)existing;
        patch.ApplyTo(model);

        var entity = (Core.Entities.CapabilityType)model;
        entity.Id = existing.Id;
        entity.Name = name; // mantém a coerência com o recurso da rota

        await repository.UpdateAsync(entity);
        return NoContent();
    }

    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCapabilityType([FromRoute] string name, [FromServices] ICapabilityTypeRepository repository)
    {
        await repository.DeleteAsync(name);
        return NoContent();
    }
}
