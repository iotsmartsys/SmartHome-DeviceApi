using Api.Models;
using Core.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/dashboards")]
[ApiController]
[DashboardRequestFilter]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status503ServiceUnavailable)]
public class DashboardController(ILogger<DashboardController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(DashboardsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDashboardsAsync([FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var dashboards = await service.GetAllAsync(cancellationToken);
        return Ok(new DashboardsResponse(dashboards.Select(dashboard => (DashboardSummary)dashboard).ToArray()));
    }

    [HttpGet("{dashboardId}", Name = "GetDashboardById")]
    [ProducesResponseType(typeof(Dashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardByIdAsync([FromRoute] long dashboardId,
        [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var dashboard = await service.GetByIdAsync(dashboardId, cancellationToken);
        var capabilities = (await service.GetCapabilitiesAsync(cancellationToken)).ToDictionary(capability => capability.CapabilityId);
        return Ok(Dashboard.FromEntity(dashboard, capabilities));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Dashboard), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> AddDashboardAsync([FromBody] DashboardCreateRequest request,
        [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var dashboard = await service.AddAsync(request.ToRequest(), cancellationToken);
        logger.LogInformation("Dashboard {DashboardId} criado", dashboard.Id);
        var response = Dashboard.FromEntity(dashboard, new Dictionary<int, Core.Entities.DashboardCapability>());
        return CreatedAtRoute("GetDashboardById", new { dashboardId = dashboard.Id }, response);
    }

    [HttpPut("{dashboardId}")]
    [ProducesResponseType(typeof(Dashboard), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> UpdateDashboardAsync([FromRoute] long dashboardId, [FromBody] DashboardUpdateRequest request,
        [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var dashboard = await service.UpdateAsync(dashboardId, request.ToRequest(), cancellationToken);
        var capabilities = (await service.GetCapabilitiesAsync(cancellationToken)).ToDictionary(capability => capability.CapabilityId);
        return Ok(Dashboard.FromEntity(dashboard, capabilities));
    }

    [HttpDelete("{dashboardId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDashboardAsync([FromRoute] long dashboardId,
        [FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(dashboardId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{dashboardId}/data")]
    [ProducesResponseType(typeof(DashboardData), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardDataAsync([FromRoute] long dashboardId,
        [FromServices] IDashboardService service, [FromServices] IDashboardDataResolver resolver, CancellationToken cancellationToken)
    {
        var dashboard = await service.GetByIdAsync(dashboardId, cancellationToken);
        var capabilities = (await service.GetCapabilitiesAsync(cancellationToken)).ToDictionary(capability => capability.CapabilityId);
        var types = await service.GetWidgetTypesAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var widgets = dashboard.Widgets.Select(widget =>
        {
            var capability = capabilities.GetValueOrDefault(widget.CapabilityId);
            return DashboardWidgetData.FromEntity(widget, capability, resolver.Resolve(capability, widget, types, now));
        }).ToArray();
        return Ok(new DashboardData(dashboard.Id, dashboard.Name, dashboard.LayoutType, DashboardTimestamp.FromInstant(now)!, widgets));
    }
}
