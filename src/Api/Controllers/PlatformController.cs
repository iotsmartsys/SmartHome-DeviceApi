using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/platforms")]
[ApiController]
public class PlatformController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Platform>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromServices] IPlatformRepository repository)
    {
        var platforms = await repository.GetAllAsync();
        var models = platforms.Select(p => (Platform)p);
        if (!models.Any())
            return NoContent();

        return Ok(models);
    }
}
