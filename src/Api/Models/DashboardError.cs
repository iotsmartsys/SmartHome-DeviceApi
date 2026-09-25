using Newtonsoft.Json;

namespace Api.Models;

public sealed record DashboardErrorResponse(DashboardError error)
{
    public static DashboardErrorResponse Create(string code, string message, string? field = null) => new(new DashboardError(code, message, new DashboardErrorDetails(field)));

    public static int GetStatusCode(string code) => code switch
    {
        "DASHBOARD_NOT_FOUND" or "WIDGET_NOT_FOUND" or "CAPABILITY_NOT_FOUND" or "DEVICE_NOT_FOUND" or "WIDGET_TYPE_NOT_FOUND" => 404,
        "WIDGET_TYPE_DISABLED" or "INVALID_WIDGET_FOR_CAPABILITY" or "UNSUPPORTED_CAPABILITY_DATA_TYPE" or "DEVICE_CAPABILITY_MISMATCH" => 422,
        "METHOD_NOT_ALLOWED" => 405,
        "UNSUPPORTED_MEDIA_TYPE" => 415,
        "DATA_SOURCE_UNAVAILABLE" => 503,
        "INTERNAL_ERROR" => 500,
        _ => 400
    };
}

public sealed record DashboardError(string code, string message, DashboardErrorDetails details);
public sealed record DashboardErrorDetails([property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? field);
