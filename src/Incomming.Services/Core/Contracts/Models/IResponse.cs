namespace Incomming.Service.Core.Contracts.Models;

public interface IResponse
{
    ResponseStatus Status { get; }
    string Message { get; }
    bool HasError { get; }
}

public interface IResponse<T> : IResponse
{
    T Data { get; }

    T GetDatPreventNull();
}