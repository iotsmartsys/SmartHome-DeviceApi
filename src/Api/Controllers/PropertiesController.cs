using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/devices/{device_id}/properties")]
[ApiController]
public class PropertiesController(ILogger<PropertiesController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Property))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] string device_id, int id, [FromServices] IPropertyRepository repository, IDeviceRepository deviceRepository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de property {id} para o device {device_id}", id, device_id);
        logger.LogInformation("Buscando property {id} para o device {device_id}", id, device_id);
        var property = await repository.GetByIdAsync(device_id, id, cancellationToken);
        if (property is null)
        {
            logger.LogWarning("Property {id} não encontrada para o device {device_id}", id, device_id);
            return NotFound();
        }

        return Ok((Property)property);
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Property>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromRoute] string device_id, [FromQuery] Property property, [FromServices] IPropertyRepository repository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de properties para o device {device_id}", device_id);
        Criteria<Core.Entities.Property> criteria = new((Core.Entities.Property)property);
        logger.LogInformation("Criteria: {criteria}", criteria);
        var properties = await repository.GetByDeviceAsync(device_id, criteria, cancellationToken);

        if (properties.Any())
            return Ok(properties.Select(p => (Property)p));

        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAsync([FromRoute] string device_id, [FromBody] Property property, [FromServices] IPropertyRepository repository, IDeviceRepository deviceRepository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de adição de property para o device {device_id}", device_id);
        var entity = (Core.Entities.Property)property;
        logger.LogInformation("Identificando device {device_id}", device_id);
        var device = await deviceRepository.GetDeviceAsync(device_id, cancellationToken);
        if (device is null)
        {
            logger.LogWarning("Device {device_id} não encontrado", device_id);
            return NotFound();
        }

        logger.LogInformation("Device: {device}", device);
        logger.LogInformation("Adicionando property {property} para o device {device_id}", entity, device_id);
        await repository.AddAsync(device_id, entity, cancellationToken);
        logger.LogInformation("Property adicionada com sucesso");

        return CreatedAtAction(nameof(GetAllAsync), new { device_id, id = entity.Id }, property);
    }

    [HttpPut()]
    [HttpPatch()]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrUpdateAsync([FromRoute] string device_id, [FromBody] Property property, [FromServices] IPropertyRepository repository, CancellationToken cancellationToken)
    {
        var entity = (Core.Entities.Property)property;
        logger.LogInformation("Request de atualização de property {property} para o device {device_id}", entity, device_id);
        logger.LogInformation("Identificando device {device_id}", device_id);
        await repository.CreateOrUpdateAsync(device_id, property, cancellationToken);
        logger.LogInformation("Property atualizada com sucesso");

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveAsync([FromRoute] string device_id, int id, [FromServices] IPropertyRepository repository, IDeviceRepository deviceRepository, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request de remoção de property {id} para o device {device_id}", id, device_id);
        logger.LogInformation("Identificando device {device_id}", device_id);
        var device = await deviceRepository.GetDeviceAsync(device_id, cancellationToken);
        if (device is null)
        {
            logger.LogWarning("Device {device_id} não encontrado", device_id);
            return NotFound();
        }

        logger.LogInformation("Device: {device}", device);
        logger.LogInformation("Removendo property {id} para o device {device_id}", id, device_id);
        await repository.RemoveAsync(device_id, new Core.Entities.Property { Id = id }, cancellationToken);
        logger.LogInformation("Property removida com sucesso");

        return NoContent();
    }
}