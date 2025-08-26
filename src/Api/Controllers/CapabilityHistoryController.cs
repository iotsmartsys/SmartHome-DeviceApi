using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/capabilities/{capability_id}/history")]
[ApiController]
public class CapabilityHistoryController(ILogger<CapabilityHistoryController> logger) : ControllerBase
{
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CapabilityHistory>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCapabilities([FromRoute] int capability_id, [FromQuery] CapabilityHistoryFind? find, [FromServices] ICappabilityHistoryRepository repository, CancellationToken cancellationToken)
    {
        var capabilities = await repository.GetByCapabilityIdAsync(capability_id, find, cancellationToken);

        return Ok(capabilities);
    }
}
