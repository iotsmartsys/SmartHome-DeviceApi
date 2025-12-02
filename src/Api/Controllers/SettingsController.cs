using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/settings")]
[ApiController]
public class SettingsController(ILogger<SettingsController> logger) : ControllerBase
{
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Settings>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllSettingsAsync([FromServices] ISettingsRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de busca de todas as settings");
        var settings = await repository.GetAllAsync(cancellationToken);
        if (settings.Any())
            return Ok(settings.Select(s => (Settings)s));

        return NoContent();
    }
}
