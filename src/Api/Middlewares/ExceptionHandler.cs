using System.Net;
using System.Text;
using MySqlConnector;

class ExceptionHandler(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<ExceptionHandler> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro inesperado");
            logger.LogCritical(ex, $"Exception: {ex}\n Inner: {ex.InnerException?.ToString()}\n StackTrace: {ex.StackTrace}");
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ExceptionHandler> logger)
    {
        logger.LogError(exception, "Ocorreu um erro inesperado");
        string message = "Ocorreu um erro inesperado ao processar a requisição.";
        string traceId = Guid.NewGuid().ToString();
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case ArgumentDomainException argument:
                message = argument.Message;
                statusCode = HttpStatusCode.BadRequest;
                break;
            case NotFoundExceptionDomain notFound:
                message = notFound.Message;
                statusCode = HttpStatusCode.NotFound;
                break;
            case MySqlException mySqlEx:
                logger.LogError(mySqlEx, "Erro de banco de dados: {Message}", mySqlEx.Message);
                message = "Erro ao processar a requisição";
                // Mapear códigos de erro MySQL mais comuns
                switch (mySqlEx.Number)
                {
                    // Duplicate entry
                    case 1062:
                        message = "Já existe um registro com o mesmo valor";
                        statusCode = HttpStatusCode.Conflict;
                        break;
                    // Cannot add or update a child row: a foreign key constraint fails / Cannot delete or update a parent row
                    case 1451:
                    case 1452:
                        message = "Violação de integridade: restrição de chave estrangeira";
                        statusCode = HttpStatusCode.BadRequest;
                        break;
                    // Lock wait timeout exceeded
                    case 1205:
                        message = "Timeout ao aguardar lock de banco de dados";
                        statusCode = HttpStatusCode.ServiceUnavailable;
                        break;
                    // Deadlock found when trying to get lock
                    case 1213:
                        message = "Deadlock detectado. Tente novamente";
                        statusCode = HttpStatusCode.ServiceUnavailable;
                        break;
                    // Too many connections
                    case 1040:
                        message = "Banco indisponível no momento (muitas conexões)";
                        statusCode = HttpStatusCode.ServiceUnavailable;
                        break;
                    // Server has gone away/ lost connection
                    case 2006:
                    case 2013:
                        message = "Conexão com o banco foi perdida";
                        statusCode = HttpStatusCode.ServiceUnavailable;
                        break;
                    default:
                        statusCode = HttpStatusCode.InternalServerError;
                        break;
                }
                break;
            default:
                break;
        }
        // Evitar escrever em response já finalizada
        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = (int)statusCode;
            context.Response.Headers["X-Trace-Id"] = traceId;
            try
            {
                await context.Response.WriteAsync(message, Encoding.UTF8);
            }
            catch (ObjectDisposedException)
            {
                // Conexão HTTP já foi encerrada, não há o que fazer
            }
        }
    }
}
