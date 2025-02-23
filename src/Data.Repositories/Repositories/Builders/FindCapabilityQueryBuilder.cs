using Core.Contracts.Repositories;
using Dapper;

namespace Data.Repositories;

internal interface IFindCapabilityQueryBuilder : ICapabilityQueryBuilder
{
    IFindCapabilityQueryBuilder WithDeviceId(string device_id);
    IFindCapabilityQueryBuilder OrderBy(string order);
    IFindCapabilityQueryBuilder OrderByDescending(string order);
}
internal class FindCapabilityQueryBuilder(CapabilityFind capabilityQuery) : CapabilityQueryBuilder(CapabilityQuery.GetCapabilitiesByDeviceAsync), IFindCapabilityQueryBuilder
{
    private string? _device_id;
    string _order_by = " ORDER BY ";

    public override CommandDefinition Build()
    {
        AddCapabilitySpecification();
        if (_order_by == " ORDER BY ")
            _order_by = "";

        _sql += _order_by;
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

    public IFindCapabilityQueryBuilder OrderBy(string order)
    {
        AddOrderBy(order);
        return this;
    }

    public IFindCapabilityQueryBuilder OrderByDescending(string order)
    {
        AddOrderBy(order, true);
        return this;
    }

    private void AddOrderBy(string name, bool orderDesc = false)
    {
        name = $"{CapabilityQuery.AliasOnQuery}.{name}";
        if (_order_by.Contains(name))
            return;

        if (_order_by == " ORDER BY ")
            _order_by += name + (orderDesc ? " DESC" : "");
        else
            _order_by += ", " + name + (orderDesc ? " DESC" : "");

    }

    void AddCapabilitySpecification()
    {
        _sql += " WHERE 1 = 1";
        if (_device_id is not null)
            _sql += " AND d.DeviceId = @device_id";
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