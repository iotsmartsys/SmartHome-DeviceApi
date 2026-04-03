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

    [HttpPut("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateValueSettingAsync(string name, [FromBody] ValueSettings settings, [FromServices] ISettingsRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Request de atualização da setting: {name}");

        await repository.SetValueAsync(name, settings.value, cancellationToken);
        return NoContent();
    }

    [HttpPut()]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateValuesSettingAsync([FromBody] IEnumerable<Settings> settings, [FromServices] ISettingsRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Request de atualização das settings");

        await repository.SetValuesAsync(settings.Select(s => (Core.Entities.Settings)s), cancellationToken);
        return NoContent();
    }

}
