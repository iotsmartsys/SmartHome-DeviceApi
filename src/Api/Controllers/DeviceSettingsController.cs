using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartHome_Api.Controllers;

[Route("api/v1/devices/{device_id}/settings")]
[ApiController]
public class DeviceSettingsController : ControllerBase
{
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Settings))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDeviceSettingsAsync([FromRoute] string device_id, [FromServices] IDeviceSettingsRepository repository, CancellationToken cancellationToken)
    {
        var settings = await repository.GetByDeviceIdAsync(device_id, cancellationToken);
        if (!settings.Any())
        {
            return NotFound($"No settings found for device with ID '{device_id}'.");
        }

        var response = DataParserHelper.ToDictionary(settings, settings.Where(s => SettingsKeyTypes.prefix_auto_format_properies_json.Is(s.Name)).SelectMany(s => s.Value.Split(',')));
        return Ok(response);
    }

    [HttpPut()]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeviceSettingsAsync([FromRoute] string device_id, [FromBody] IEnumerable<Settings> settings, [FromServices] IDeviceSettingsRepository repository, CancellationToken cancellationToken)
    {
        if (settings == null || !settings.Any())
        {
            return BadRequest("Settings cannot be null or empty.");
        }

        await repository.SaveAsync(device_id, settings.Select(s => (Core.Entities.Settings)s), cancellationToken);

        return NoContent();
    }


}