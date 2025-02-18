using Incomming.Service.Core.Contracts.Models;
using Incomming.Service.Core.Exceptions;
using MediatR;

namespace Incomming.Service.Core.Models;

public class IncommingDevice : IRequest<IResponse>
{
    public string? device_id { get; set; }
    public string? capability_name { get; set; }
    public string? value { get; set; }

    public void ThrowIfInvalid()
    {
        if (string.IsNullOrWhiteSpace(device_id))
            throw new InvalidEventException("device_id não pode ser nulo ou vazio");

        if (string.IsNullOrWhiteSpace(capability_name))
            throw new InvalidEventException("capability_name não pode ser nulo ou vazio");

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEventException("value não pode ser nulo ou vazio");
    }

    internal string GetCapabilityName()
    {
        ThrowIfInvalid();
        return capability_name!;
    }
}

public record class Command<T>(T Data) : IRequest<IResponse>;