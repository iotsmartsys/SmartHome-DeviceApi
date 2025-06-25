using System.Net;
using System.Text;
using MySql.Data.MySqlClient;

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
                if (mySqlEx.Message.Contains("Duplicate entry"))
                {
                    message = "Já existe um registro com o mesmo valor";
                    statusCode = HttpStatusCode.Conflict;
                }
                else if (mySqlEx.Message.Contains("Cannot add or update a child row: a foreign key constraint fails"))
                {
                    message = "Não é possível adicionar ou atualizar o registro devido a uma restrição de chave estrangeira";
                    statusCode = HttpStatusCode.BadRequest;
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                }
                break;
            default:
                break;
        }
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)statusCode;
        context.Response.Headers.Append("X-Trace-Id", traceId);

        await context.Response.WriteAsync(message, Encoding.UTF8);
    }
}
