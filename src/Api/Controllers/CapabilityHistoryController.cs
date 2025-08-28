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
    public async Task<IActionResult> GetAllCapabilities([FromRoute] int capability_id, [FromQuery] CapabilityHistoryFind? find, [FromServices] ICappabilityHistoryRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetByCapabilityIdAsync(capability_id, find, cancellationToken);

        return Ok(capabilities);
    }

    // criação por capability_name na rota absoluta
    [HttpPost("~/api/v1/capabilities/{capability_name}/history")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCapabilityHistory(
        [FromRoute] string capability_name,
        [FromBody] Api.Models.CapabilityHistoryCreate input,
        [FromServices] ICappabilityHistoryRepository repository,
        [FromServices] ICapabilityRepository capabilityRepository,
        CancellationToken cancellationToken)
    {
        if (input is null || string.IsNullOrWhiteSpace(input.value))
            return BadRequest();

        // valida existência e obtém id para Location
        var capability = await capabilityRepository.GetByNameAsync(cancellationToken, capability_name);
        if (capability is null)
            return NotFound();

        await repository.AddAsync(capability_name, input.value, cancellationToken);
        return CreatedAtRoute("GetCapabilityHistory", new { capability_id = capability.Id }, null);
    }
}
