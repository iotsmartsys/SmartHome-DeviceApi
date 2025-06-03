using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartHome_Api.Controllers
{
    [Route("api/v1/devices")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Device>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDevices([FromServices] IDeviceRepository repository, CancellationToken cancellationToken)
        {
            var devices = await repository.GetDevicesAsync(cancellationToken);
            var models = devices.Select(d => {
                d.ClearCapabilities();
                d.ClearProperties();
                var model = (Device)d;
                return model;
            });
            return Ok(models);
        }

        [HttpGet("{device_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Device))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDevice([FromRoute] string device_id, [FromServices] IDeviceRepository repository, CancellationToken cancellationToken)
        {
            var device = await repository.GetDeviceAsync(device_id, cancellationToken);
            if (device == null)
                return NotFound();
            return Ok((Device)device);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDevice([FromBody] Device device, [FromServices] IDeviceRepository repository, CancellationToken cancellationToken)
        {
            var entity = (Core.Entities.Device)device;
            await repository.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(GetDevice), new { device_id = entity.DeviceId }, null);
        }
    }
}
