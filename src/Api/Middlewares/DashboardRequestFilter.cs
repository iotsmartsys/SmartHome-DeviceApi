using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

// Runs before ApiController's automatic 400/415 filters, only on Dashboard endpoints.
public sealed class DashboardRequestFilter : ActionFilterAttribute
{
    public DashboardRequestFilter() => Order = -3000;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        if (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method))
        {
            var mediaType = request.ContentType?.Split(';')[0].Trim();
            if (mediaType is null || !(mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
                mediaType.StartsWith("application/", StringComparison.OrdinalIgnoreCase) && mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new ObjectResult(DashboardErrorResponse.Create("UNSUPPORTED_MEDIA_TYPE", "O corpo deve usar JSON."))
                { StatusCode = StatusCodes.Status415UnsupportedMediaType };
                return;
            }
        }
        foreach (var name in new[] { "dashboardId", "widgetId", "capabilityId" })
        {
            if (context.RouteData.Values.TryGetValue(name, out var value) &&
                (value?.ToString() is not { Length: > 0 } text || text.Any(character => character is < '0' or > '9')))
                context.ModelState.AddModelError(name, "Identificador inválido.");
        }
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.SelectMany(entry => entry.Value!.Errors.Select(error => (entry.Key, Error: error))).ToArray();
            var unsupported = errors.Any(entry => Find<UnsupportedContentTypeException>(entry.Error.Exception) is not null);
            var json = errors.Select(entry => Find<DashboardJsonException>(entry.Error.Exception)).FirstOrDefault(error => error is not null);
            var code = unsupported ? "UNSUPPORTED_MEDIA_TYPE" : json?.Code ?? "INVALID_REQUEST";
            var field = unsupported ? null : json?.Field ?? errors.FirstOrDefault().Key ?? "request";
            context.Result = new ObjectResult(DashboardErrorResponse.Create(code, "Requisição inválida.", field))
            { StatusCode = DashboardErrorResponse.GetStatusCode(code) };
            return;
        }
        foreach (var model in context.ActionArguments.Values.OfType<ISelfValidate>()) model.Validate();
    }

    private static T? Find<T>(Exception? exception) where T : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
            if (current is T match) return match;
        return null;
    }
}
