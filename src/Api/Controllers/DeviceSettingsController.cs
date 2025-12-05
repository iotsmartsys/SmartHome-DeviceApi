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

        var response = DataParserHelper.ToDictionary(settings, settings.Where(s => SettingsKeyTypes.prefix_auto_format_properies_json.Is(s.Name)).SelectMany(s => s.Value.Split(',')));
        return Ok(response);
    }

    [HttpPut()]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeviceSettingsAsync([FromRoute] string device_id, [FromBody] Settings model, [FromServices] IDeviceSettingsRepository repository, CancellationToken cancellationToken)
    {
        await repository.SaveAsync(device_id, model, cancellationToken);

        return NoContent();
    }


}