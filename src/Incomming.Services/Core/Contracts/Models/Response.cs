namespace Incomming.Service.Core.Contracts.Models;

public class Response : IResponse
{
    public ResponseStatus Status { get; protected set; }
    public string Message { get; protected set; }
    public bool HasError => Status == ResponseStatus.Error;

    public Response()
    {
        Status = ResponseStatus.Success;
        Message = string.Empty;
    }

    public Response(ResponseStatus status, string message)
    {
        Status = status;
        Message = message;
    }

    public IResponse Append(IResponse response)
    {
        if (response.HasError)
        {
            Status = ResponseStatus.Error;
            Message = response.Message;
        }

        return this;
    }
}
public class Response<T> : Response, IResponse<T>
{
    public T Data { get; private set; }

    public Response()
    {
        Data = default!;
    }

    public Response(T data)
    {
        Data = data;
    }

    public Response(ResponseStatus status, string message) : base(status, message)
    {
        Data = default!;
    }

    public Response(T data, ResponseStatus status, string message) : base(status, message)
    {
        Data = data;
    }

    public new IResponse<T> Append(IResponse response)
    {
        base.Append(response);
        return this;
    }

    public T GetDatPreventNull()
    {
        if (Data == null)
            throw new ArgumentNullException($"{nameof(Data)} is null");

        return Data;
    }
}