using System.Net;

namespace Incomming.Service.Core.Contracts.Models;

public class ApiResponse<T> : Response<T>
{
    public ApiResponse()
    {
    }

    public ApiResponse(T data) : base(data)
    {
    }

    public ApiResponse(ResponseStatus status, string message) : base(status, message)
    {
    }
    public ApiResponse(HttpStatusCode status, string message)
    {
        Status = status switch
        {
            HttpStatusCode.OK => ResponseStatus.Success,
            HttpStatusCode.NotFound or
            HttpStatusCode.NoContent
            => ResponseStatus.ThereIsNothingToBeDone,
            HttpStatusCode.BadRequest => ResponseStatus.Invalid,
            _ => ResponseStatus.Error
        };
        Message = message;
    }

    public ApiResponse(T data, ResponseStatus status, string message) : base(data, status, message)
    {
    }
}