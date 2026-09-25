using Api.Models;
using Core.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/dashboard-widget-types")]
[ApiController]
[DashboardRequestFilter]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status503ServiceUnavailable)]
public class DashboardWidgetTypeController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(DashboardWidgetTypesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDashboardWidgetTypesAsync([FromServices] IDashboardService service, CancellationToken cancellationToken)
    {
        var types = await service.GetWidgetTypesAsync(cancellationToken);
        return Ok(new DashboardWidgetTypesResponse(types.Select(type => (DashboardWidgetType)type).ToArray()));
    }
}
