using Api.Models;
using Core.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/devices/{deviceId}/metrics")]
[ApiController]
public sealed class DeviceMetricsController(ILogger<DeviceMetricsController> logger) : ControllerBase
{
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Save(
        [FromRoute] string deviceId,
        [FromBody] DeviceMetricsRequest request,
        [FromServices] IDeviceMetricsRepository repository,
        CancellationToken cancellationToken)
    {
        if (!await repository.SaveAsync(deviceId, request.ToEntity(), cancellationToken))
            return NotFound();

        return NoContent();
    }

    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeviceMetricsCurrentResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(
        [FromRoute] string deviceId,
        [FromServices] IDeviceMetricsRepository repository,
        CancellationToken cancellationToken)
    {
        var metrics = await repository.GetCurrentAsync(deviceId, cancellationToken);
        if (metrics is null)
        {
            logger.LogWarning("Métricas atuais não encontradas para o dispositivo {DeviceId}", deviceId);
            return NotFound();
        }

        return Ok(DeviceMetricsCurrentResponse.FromEntity(metrics));
    }

    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeviceMetricsHistoryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        [FromRoute] string deviceId,
        [FromServices] IDeviceMetricsRepository repository,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) ModelState.AddModelError(nameof(page), "page deve ser maior ou igual a 1");
        if (pageSize is < 1 or > 500) ModelState.AddModelError(nameof(pageSize), "pageSize deve estar entre 1 e 500");
        if (from > to) ModelState.AddModelError(nameof(from), "from não pode ser posterior a to");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!await repository.DeviceExistsAsync(deviceId, cancellationToken))
            return NotFound();

        var result = await repository.GetHistoryAsync(deviceId, from, to, page, pageSize, cancellationToken);
        var response = new DeviceMetricsHistoryResponse(
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalItems == 0 ? 0 : (int)Math.Ceiling(result.TotalItems / (double)result.PageSize),
            result.Items.Select(item => new DeviceMetricsHistoryItemResponse(
                item.UptimeMs, item.CpuPercent, item.MemoryPercent, item.TemperatureC,
                item.FrequencyMhz, item.Rssi, item.ReceivedAt)).ToArray());
        return Ok(response);
    }
}
