using Api.Models;
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

        return Ok(capabilities);
    }

    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CapabilityType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityType([FromServices] ICapabilityTypeRepository repository, string name)
    {
        var capability = await repository.GetByNameAsync(name);
        if (capability is null)
            return NotFound();

        return Ok(capability);
    }
}