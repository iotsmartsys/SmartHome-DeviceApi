using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/places")]
[ApiController]
public class MonitoredPlaceController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MonitoredPlace>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromServices] IMonitoredPlaceRepository repository)
    {
        var places = await repository.GetAllAsync();
        var models = places.Select(p => (MonitoredPlace)p);
        if (!models.Any())
            return NoContent();

        return Ok(models);
    }
}