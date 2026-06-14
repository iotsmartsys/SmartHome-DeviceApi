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
    public async Task<IActionResult> GetDeviceSettingsAsync([FromRoute] string device_id, [FromQuery] DeviceSettingsQuery? query, [FromServices] IDeviceSettingsRepository repository, CancellationToken cancellationToken)
    {
        var settings = await repository.GetByDeviceIdAsync(device_id, cancellationToken);
        if (!settings.Any())
        {
            return NotFound($"No settings found for device with ID '{device_id}'.");
        }
        string[] prefixAutoFormatProperties = query?.prefix_auto_format_properties_json?.Split(',') ?? [.. settings.Where(s => SettingsKeyTypes.prefix_auto_format_properies_json.Is(s.Name)).SelectMany(s => s.Value.Split(','))];

        switch (query?.use_key_value)
        {
            case "true":
            case "yes":
                return Ok(settings.Select(s => (Settings)s));
            default:
                var response = DataParserHelper.ToDictionary(settings, prefixAutoFormatProperties);
                return Ok(response);
        }
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
public record class DeviceSettingsQuery(string? prefix_auto_format_properties_json, string? use_key_value);