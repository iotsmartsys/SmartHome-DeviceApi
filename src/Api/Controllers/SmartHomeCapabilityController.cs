using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/smart-home/{smart_home_id}/capabilities")]
[ApiController]
public class SmartHomeCapabilityController : ControllerBase
{
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SmartHomeCapability))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCapabilities([FromRoute] string smart_home_id, [FromQuery] CapabilityFind? capabilityFind, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        if (capabilityFind == null)
            capabilityFind = new CapabilityFind();

        capabilityFind = capabilityFind with { smart_home_id = smart_home_id };
        var capabilities = await repository.GetAllCapabilitiesAsync(capabilityFind, cancellationToken);
        if (capabilities.Any())
            return Ok(capabilities.Select(c => (SmartHomeCapability)c));

        return NoContent();
    }

    [HttpGet("{uid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SmartHomeCapability))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUidCapabilities([FromRoute] string smart_home_id, [FromRoute] string uid, [FromServices] ICapabilityRepository repository, CancellationToken cancellationToken)
    {
        var capability = await repository.GetByUidAsync(uid, smart_home_id, cancellationToken);
        if (capability != null)
            return Ok((SmartHomeCapability)capability);

        return NotFound();
    }
}