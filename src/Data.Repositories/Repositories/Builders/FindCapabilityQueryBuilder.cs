using Core.Contracts.Repositories;
using Dapper;

namespace Data.Repositories;

internal interface IFindCapabilityQueryBuilder : ICapabilityQueryBuilder
{
    IFindCapabilityQueryBuilder WithDeviceId(string device_id);
}
internal class FindCapabilityQueryBuilder(CapabilityFind capabilityQuery) : CapabilityQueryBuilder(CapabilityQuery.GetCapabilitiesByDeviceAsync), IFindCapabilityQueryBuilder
{
    private string? _device_id;

    public override CommandDefinition Build()
    {
        AddCapabilitySpecification();
        return new CommandDefinition(_sql, new
        {
            device_id = _device_id,
            name = capabilityQuery.name,
            type = capabilityQuery.type,
            owner = capabilityQuery.owner,
            value = capabilityQuery.value
        },
        transaction: _transaction,
        cancellationToken: cancellationToken);
    }

    public IFindCapabilityQueryBuilder WithDeviceId(string device_id)
    {
        _device_id = device_id;
        return this;
    }

    void AddCapabilitySpecification()
    {
        _sql += " WHERE 1 = 1";
        if (_device_id is not null)
            _sql += " AND d.DeviceId = @_device_id";
        if (capabilityQuery.name is not null)
            _sql += " AND dc.Name = @name";
        if (capabilityQuery.type is not null)
            _sql += " AND c.Name = @type";
        if (capabilityQuery.owner is not null)
            _sql += " AND dc.deviceOwner = @owner";
        if (capabilityQuery.value is not null)
            _sql += " AND dc.Value = @value";
    }
}