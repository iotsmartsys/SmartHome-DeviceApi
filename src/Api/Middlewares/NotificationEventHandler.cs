class NotificationEventHandler(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<NotificationEventHandler> logger, INotificationEventFacade facade)
    {
        try
        {
            await _next(context);
            if (context.Request.Path.HasValue)
            {
                string path = context.Request.Path.Value;
                switch (context.Response.StatusCode)
                {
                    case 201:
                        if (path.StartsWith("/api/v1/capability"))
                        {
                            await facade.PublishUpdateChangeAsync(new CapabilityAddedOrUpdateEvent(Guid.NewGuid().ToString()), CancellationToken.None);
                        }
                        break;
                    case 400:
                        logger.LogWarning("Bad Request: {Path}", context.Request.Path);
                        break;
                    case 404:
                        logger.LogWarning("Not Found: {Path}", context.Request.Path);
                        break;
                    default:
                        logger.LogInformation("Response {StatusCode} for {Path}", context.Response.StatusCode, context.Request.Path);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro inesperado");
            logger.LogCritical(ex, $"Exception: {ex}\n Inner: {ex.InnerException?.ToString()}\n StackTrace: {ex.StackTrace}");
        }
    }

}