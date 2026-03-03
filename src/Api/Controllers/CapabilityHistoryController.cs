using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/capabilities/{capability_id}/history")]
[ApiController]
public class CapabilityHistoryController(ILogger<CapabilityHistoryController> logger) : ControllerBase
{
    [HttpGet(Name = "GetCapabilityHistory")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CapabilityHistory>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCapabilities([FromRoute] int capability_id, [FromQuery] CapabilityHistoryFind? find, [FromServices] ICapabilityHistoryRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetByCapabilityIdAsync(capability_id, find, cancellationToken);

        return Ok(capabilities);
    }

    [HttpPost("~/api/v1/capabilities/{capability_id}/history")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCapabilityHistory(
        [FromRoute] int capability_id,
        [FromBody] Api.Models.CapabilityHistoryCreate input,
        [FromServices] ICapabilityHistoryRepository repository,
        [FromServices] ICapabilityRepository capabilityRepository,
        CancellationToken cancellationToken)
    {
        if (input is null || string.IsNullOrWhiteSpace(input.value))
            return BadRequest();

        var capability = await capabilityRepository.GetByIdAsync(capability_id, cancellationToken);
        if (capability is null)
            return NotFound();

        await repository.AddAsync(capability.Name, input.value, cancellationToken);
        return CreatedAtRoute("GetCapabilityHistory", new { capability_id = capability.Id }, null);
    }
}
