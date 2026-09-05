using System.Text.Json;
using System.Text.RegularExpressions;
using Core.Exceptions;
using MySqlConnector;

// Restricted to Dashboard routes. Existing endpoints retain ExceptionHandler and MVC defaults.
public sealed class DashboardExceptionHandler(RequestDelegate next, ILogger<DashboardExceptionHandler> logger)
{
    private static readonly Regex Routes = new(@"^/api/v1/(?:dashboards(?:/[^/]+(?:/data|/widgets(?:/[^/]+)?)?)?|dashboard-widget-types|dashboard-capabilities(?:/[^/]+/compatible-widgets)?)/?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    public async Task InvokeAsync(HttpContext context)
    {
        if (!Routes.IsMatch(context.Request.Path.Value ?? "")) { await next(context); return; }
        try
        {
            if (context.Request.Query.Count != 0)
                throw new DashboardException("INVALID_REQUEST", 400, "Esta rota não aceita filtros de consulta.", context.Request.Query.Keys.First());
            await next(context);
            if (!context.Response.HasStarted && context.Response.StatusCode is 405 or 415)
            {
                var status = context.Response.StatusCode;
                await Write(context, status, status == 405 ? "METHOD_NOT_ALLOWED" : "UNSUPPORTED_MEDIA_TYPE",
                    status == 405 ? "Método não suportado." : "Tipo de mídia não suportado.", null);
            }
        }
        catch (DashboardException ex) when (!context.Response.HasStarted)
        {
            await Write(context, ex.StatusCode, ex.Code, ex.Message, ex.Field);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The caller disconnected; do not manufacture an HTTP success or an operational failure.
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var unavailable = ex is TimeoutException || ex is MySqlException sql &&
                (sql.IsTransient || sql.Number is 1040 or 1042 or 1043 or 2002 or 2003 or 2006 or 2013 or -1);
            logger.LogError("Dashboard request failed ({ExceptionType}); unavailable={Unavailable}", ex.GetType().Name, unavailable);
            await Write(context, unavailable ? 503 : 500, unavailable ? "DATA_SOURCE_UNAVAILABLE" : "INTERNAL_ERROR",
                unavailable ? "Fonte de dados indisponível." : "Falha ao processar a requisição.", null);
        }
    }
    private static Task Write(HttpContext context, int status, string code, string message, string? field)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        // No response/body from a previously failed write should be advertised as a created resource.
        context.Response.Headers.Remove("Location");
        object details = field is null ? new Dictionary<string, object>() : new { field };
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = new { code, message, details } }), context.RequestAborted);
    }
}
