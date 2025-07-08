using Core.Contracts.Repositories;
using Dapper;

namespace Data.Repositories;

internal interface IFindCapabilityQueryBuilder : ICapabilityQueryBuilder
{
    IFindCapabilityQueryBuilder WithId(int id);
    IFindCapabilityQueryBuilder WithActive(bool active);
    IFindCapabilityQueryBuilder WithName(string name);
    IFindCapabilityQueryBuilder WithType(string type);
    IFindCapabilityQueryBuilder WithOwner(string owner);
    IFindCapabilityQueryBuilder WithValue(string value);
    IFindCapabilityQueryBuilder WithReferenceId(string referenceId);
    IFindCapabilityQueryBuilder OrderByDescending(string order);
}
internal class FindCapabilityQueryBuilder() : CapabilityQueryBuilder(CapabilityQuery.GetAllCapabilities), IFindCapabilityQueryBuilder
{
    private int? id;
    private bool? active;
    private string? name;
    private string? type;
    private string? owner;
    private string? value;
    private string? referenceId;

    string _order_by = " ORDER BY ";

    public override CommandDefinition Build()
    {
        AddCapabilitySpecification();
        if (_order_by == " ORDER BY ")
            _order_by = "";

        _sql += _order_by;
        return new CommandDefinition(_sql, new
        {
            name = name,
            type = type,
            owner = owner,
            value = value,
            active = active,
            referenceId = referenceId,
        },
        transaction: _transaction,
        cancellationToken: cancellationToken);
    }

    public IFindCapabilityQueryBuilder WithReferenceId(string referenceId)
    {
        this.referenceId = referenceId;
        _sql += " AND crsp.ReferenceId = @referenceId";
        return this;
    }

    public IFindCapabilityQueryBuilder WithId(int id)
    {
        this.id = id;
        return this;
    }

    public IFindCapabilityQueryBuilder WithActive(bool active)
    {
        this.active = active;
        return this;
    }

    public IFindCapabilityQueryBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public IFindCapabilityQueryBuilder WithType(string type)
    {
        this.type = type;
        return this;
    }
    
    public IFindCapabilityQueryBuilder WithOwner(string owner)
    {
        this.owner = owner;
        return this;
    }

    public IFindCapabilityQueryBuilder WithValue(string value)
    {
        this.value = value;
        return this;
    }

    public IFindCapabilityQueryBuilder OrderByDescending(string order)
    {
        AddOrderBy(order, true);
        return this;
    }

    private void AddOrderBy(string name, bool orderDesc = false)
    {
        name = $"dc.{name}";
        if (_order_by.Contains(name))
            return;

        if (_order_by == " ORDER BY ")
            _order_by += name + (orderDesc ? " DESC" : "");
        else
            _order_by += ", " + name + (orderDesc ? " DESC" : "");

    }

    void AddCapabilitySpecification()
    {
        if (id.HasValue)
            _sql += " AND dc.Id = @id";
        if (name is not null)
            _sql += " AND dc.Name = @name";
        if (type is not null)
            _sql += " AND ct.Name = @type";
        if (owner is not null)
            _sql += " AND dc.deviceOwner = @owner";
        if (value is not null)
            _sql += " AND dc.Value = @value";
        if (active.HasValue)
            _sql += " AND dc.Active = @active";
    }

    internal IFindCapabilityQueryBuilder WithFind(CapabilityFind? capabilityFind)
    {
        if (capabilityFind is null)
            return this;

        if (capabilityFind.name is not null)
            WithName(capabilityFind.name);
        if (capabilityFind.type is not null)
            WithType(capabilityFind.type);
        if (capabilityFind.owner is not null)
            WithOwner(capabilityFind.owner);
        if (capabilityFind.value is not null)
            WithValue(capabilityFind.value);
        if (capabilityFind.active.HasValue)
            WithActive(capabilityFind.active.Value);
        if (capabilityFind.reference_id is not null)
            WithReferenceId(capabilityFind.reference_id);

        return this;
    }
}