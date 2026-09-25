using Api.Models;
using Core.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/dashboard-capabilities")]
[ApiController]
[DashboardRequestFilter]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(DashboardErrorResponse), StatusCodes.Status503ServiceUnavailable)]
public class DashboardCapabilityController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(DashboardCapabilitiesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDashboardCapabilitiesAsync([FromServices] IDashboardService service,
        [FromServices] IDashboardDataResolver resolver, [FromServices] IDashboardWidgetCompatibilityResolver compatibility,
        CancellationToken cancellationToken)
    {
        var capabilities = await service.GetCapabilitiesAsync(cancellationToken);
        var types = await service.GetWidgetTypesAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        return Ok(new DashboardCapabilitiesResponse(capabilities.Select(capability => DashboardCapability.FromEntity(capability,
            resolver.Resolve(capability, null, types, now), compatibility.GetCompatibleWidgets(capability, types))).ToArray()));
    }

    [HttpGet("{capabilityId}/compatible-widgets")]
    [ProducesResponseType(typeof(DashboardCompatibleWidgetsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompatibleDashboardWidgetsAsync([FromRoute] int capabilityId,
        [FromServices] IDashboardService service, [FromServices] IDashboardWidgetCompatibilityResolver compatibility,
        CancellationToken cancellationToken)
    {
        var capability = await service.GetCapabilityByIdAsync(capabilityId, cancellationToken);
        var types = await service.GetWidgetTypesAsync(cancellationToken);
        var compatible = compatibility.GetCompatibleWidgets(capability, types).Select(type => (DashboardWidgetType)type).ToArray();
        return Ok(new DashboardCompatibleWidgetsResponse(capability.CapabilityId, capability.CapabilityCode, capability.DataType, compatible));
    }
}
