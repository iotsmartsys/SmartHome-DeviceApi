using Api.Models;
using Core.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/dashboards/{dashboardId}/widgets")]
[ApiController]
[DashboardRequestFilter]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status503ServiceUnavailable)]
public class DashboardWidgetController(ILogger<DashboardWidgetController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(DashboardWidget), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> AddDashboardWidgetAsync([FromRoute] long dashboardId, [FromBody] DashboardWidgetCreateRequest request,
        [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var widget = await service.AddWidgetAsync(dashboardId, request.ToRequest(), cancellationToken);
        var capabilities = (await service.GetCapabilitiesAsync(cancellationToken)).ToDictionary(capability => capability.CapabilityId);
        logger.LogInformation("Widget {WidgetId} criado no Dashboard {DashboardId}", widget.Id, dashboardId);
        return CreatedAtRoute("GetDashboardById", new { dashboardId }, DashboardWidget.FromEntity(widget, capabilities.GetValueOrDefault(widget.CapabilityId)));
    }

    [HttpPut("{widgetId}")]
    [ProducesResponseType(typeof(DashboardWidget), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> UpdateDashboardWidgetAsync([FromRoute] long dashboardId, [FromRoute] long widgetId,
        [FromBody] DashboardWidgetUpdateRequest request, [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var widget = await service.UpdateWidgetAsync(dashboardId, widgetId, request.ToRequest(), cancellationToken);
        var capabilities = (await service.GetCapabilitiesAsync(cancellationToken)).ToDictionary(capability => capability.CapabilityId);
        return Ok(DashboardWidget.FromEntity(widget, capabilities.GetValueOrDefault(widget.CapabilityId)));
    }

    [HttpDelete("{widgetId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDashboardWidgetAsync([FromRoute] long dashboardId, [FromRoute] long widgetId,
        [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        await service.DeleteWidgetAsync(dashboardId, widgetId, cancellationToken);
        return NoContent();
    }
}
