using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Api.Models;
using Core.Exceptions;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1")]
public sealed class DashboardController(DashboardService service, DashboardDataResolver resolver,
    DashboardWidgetCompatibilityResolver compatibility) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [HttpGet("dashboards")]
    public async Task<IActionResult> List(CancellationToken ct) => Json(new { Items = (await service.ListAsync(ct)).Select(DashboardResponse.Summary).ToArray() });

    [HttpGet("dashboards/{dashboardId}")]
    public async Task<IActionResult> Get(string dashboardId, CancellationToken ct)
    {
        var dashboard = await service.GetAsync(Id(dashboardId), ct);
        var sources = (await service.SourcesAsync(ct)).ToDictionary(s => s.CapabilityId);
        return Json(DashboardResponse.Dashboard(dashboard, sources));
    }

    [HttpPost("dashboards")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var dashboard = await service.SaveDashboardAsync(null, await Body(ct), ct);
        Response.Headers.Location = $"/api/v1/dashboards/{dashboard.Id.ToString(CultureInfo.InvariantCulture)}";
        return Json(DashboardResponse.Dashboard(dashboard, new Dictionary<int, Core.Entities.DashboardCapabilitySource>()), 201);
    }

    [HttpPut("dashboards/{dashboardId}")]
    public async Task<IActionResult> Update(string dashboardId, CancellationToken ct)
    {
        var dashboard = await service.SaveDashboardAsync(Id(dashboardId), await Body(ct), ct);
        var sources = (await service.SourcesAsync(ct)).ToDictionary(s => s.CapabilityId);
        return Json(DashboardResponse.Dashboard(dashboard, sources));
    }

    [HttpDelete("dashboards/{dashboardId}")]
    public async Task<IActionResult> Delete(string dashboardId, CancellationToken ct)
    {
        await service.DeleteDashboardAsync(Id(dashboardId), ct);
        return NoContent();
    }

    [HttpPost("dashboards/{dashboardId}/widgets")]
    public async Task<IActionResult> CreateWidget(string dashboardId, CancellationToken ct)
    {
        var parent = Id(dashboardId);
        var widget = await service.SaveWidgetAsync(parent, null, await Body(ct), ct);
        Response.Headers.Location = $"/api/v1/dashboards/{parent.ToString(CultureInfo.InvariantCulture)}";
        var sources = (await service.SourcesAsync(ct)).ToDictionary(s => s.CapabilityId);
        return Json(DashboardResponse.Widget(widget, sources.GetValueOrDefault(widget.CapabilityId)), 201);
    }

    [HttpPut("dashboards/{dashboardId}/widgets/{widgetId}")]
    public async Task<IActionResult> UpdateWidget(string dashboardId, string widgetId, CancellationToken ct)
    {
        var widget = await service.SaveWidgetAsync(Id(dashboardId), Id(widgetId), await Body(ct), ct);
        var sources = (await service.SourcesAsync(ct)).ToDictionary(s => s.CapabilityId);
        return Json(DashboardResponse.Widget(widget, sources.GetValueOrDefault(widget.CapabilityId)));
    }

    [HttpDelete("dashboards/{dashboardId}/widgets/{widgetId}")]
    public async Task<IActionResult> DeleteWidget(string dashboardId, string widgetId, CancellationToken ct)
    {
        await service.DeleteWidgetAsync(Id(dashboardId), Id(widgetId), ct);
        return NoContent();
    }

    [HttpGet("dashboard-widget-types")]
    public async Task<IActionResult> Types(CancellationToken ct) => Json(new { Items = (await service.TypesAsync(ct)).Select(DashboardResponse.Type).ToArray() });

    [HttpGet("dashboard-capabilities")]
    public async Task<IActionResult> Capabilities(CancellationToken ct)
    {
        var sources = await service.SourcesAsync(ct);
        var types = await service.TypesAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return Json(new { Items = sources.Select(s => DashboardResponse.Capability(s,
            resolver.Resolve(s, null, types, now), compatibility.Compatible(s, types))).ToArray() });
    }

    [HttpGet("dashboard-capabilities/{capabilityId}/compatible-widgets")]
    public async Task<IActionResult> Compatible(string capabilityId, CancellationToken ct)
    {
        var id = Id(capabilityId);
        if (id > int.MaxValue) throw Invalid("capabilityId");
        var source = await service.SourceAsync((int)id, ct);
        return Json(new
        {
            source.CapabilityId, source.CapabilityCode, DataType = DashboardWidgetCompatibilityResolver.VisualType(source),
            CompatibleWidgets = compatibility.Compatible(source, await service.TypesAsync(ct)).Select(DashboardResponse.Type).ToArray()
        });
    }

    [HttpGet("dashboards/{dashboardId}/data")]
    public async Task<IActionResult> Data(string dashboardId, CancellationToken ct)
    {
        var dashboard = await service.GetAsync(Id(dashboardId), ct);
        var sources = (await service.SourcesAsync(ct)).ToDictionary(s => s.CapabilityId);
        var types = await service.TypesAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return Json(new
        {
            DashboardId = dashboard.Id, dashboard.Name, dashboard.LayoutType, GeneratedAt = DashboardResponse.Instant(now),
            Widgets = dashboard.Widgets.Select(w =>
            {
                var source = sources.GetValueOrDefault(w.CapabilityId);
                return DashboardResponse.RenderedWidget(w, source, resolver.Resolve(source, w, types, now));
            }).ToArray()
        });
    }

    private ContentResult Json(object value, int status = 200) => new()
    { StatusCode = status, ContentType = "application/json; charset=utf-8", Content = JsonSerializer.Serialize(value, JsonOptions) };
    private static long Id(string text)
    {
        if (!long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0) throw Invalid("id");
        return id;
    }
    private static DashboardException Invalid(string field) => new("INVALID_REQUEST", 400, "Requisição inválida.", field);
    private async Task<JsonObject> Body(CancellationToken ct)
    {
        var mediaType = Request.ContentType?.Split(';')[0].Trim();
        if (mediaType is null || !(mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.StartsWith("application/", StringComparison.OrdinalIgnoreCase) && mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
            throw new DashboardException("UNSUPPORTED_MEDIA_TYPE", 415, "O corpo deve usar JSON.");
        try
        {
            using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: ct);
            CheckDuplicates(document.RootElement);
            return JsonNode.Parse(document.RootElement.GetRawText()) as JsonObject ?? throw Invalid("request");
        }
        catch (JsonException) { throw Invalid("request"); }
    }
    private static void CheckDuplicates(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw Invalid(property.Name);
                CheckDuplicates(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) CheckDuplicates(item);
    }
}
