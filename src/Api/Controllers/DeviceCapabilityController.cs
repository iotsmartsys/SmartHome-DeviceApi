using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/devices/{device_id}/capabilities")]
[ApiController]
public class DeviceCapabilityController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilities([FromRoute] string device_id, [FromServices] IDeviceCapabilityRepository repository)
    {
        var capabilities = await repository.GetCapabilitiesByDeviceAsync(device_id);
        if (capabilities.Any())
            return Ok(capabilities.Select(c => (Capability)c));

        return NotFound();
    }

    [HttpGet("{capability_name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Capability>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCapabilityByName([FromRoute] string device_id, [FromRoute] string capability_name, [FromServices] IDeviceCapabilityRepository repository)
    {
        var capability = await repository.GetByDeviceAndNameAsync(device_id, capability_name);
        if (capability is not null)
            return Ok((Capability)capability);

        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCapabilities([FromRoute] string device_id, [FromBody] IEnumerable<Capability> capabilities, [FromServices] IDeviceCapabilityRepository repository)
    {
        await repository.AddForDeviceAsync(device_id, capabilities.Select(c => (Core.Entities.Capability)c));
        return NoContent();
    }

    [HttpPatch()]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCapabilities([FromRoute] string device_id, [FromBody] CapabilityUpdate capability, [FromServices] IDeviceCapabilityRepository repository)
    {
        await repository.UpdateForDeviceAsync(device_id, (Core.Entities.Capability)capability);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCapabilities([FromRoute] string device_id, [FromBody] IEnumerable<Capability> capabilities, [FromServices] IDeviceCapabilityRepository repository)
    {
        await repository.RemoveFromDeviceAsync(device_id, capabilities.Select(c => (Core.Entities.Capability)c));
        return NoContent();
    }
}