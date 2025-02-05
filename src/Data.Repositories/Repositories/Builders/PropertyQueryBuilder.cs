using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;

namespace Data.Repositories;

internal class PropertyQueryBuilder(string device_id)
{
    string _sql = PropertyQuery.GetAll;
    int _id;
    string? _name;
    string? _value;
    string? _description;
    private CancellationToken _cancellationToken = default;
    private IDbTransaction? _transaction;

    public PropertyQueryBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        return this;
    }

    public PropertyQueryBuilder WithTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        return this;
    }

    public PropertyQueryBuilder WithName(string name)
    {
        _sql += " dp.name = @name ";
        _name = name;

        return this;
    }

    public PropertyQueryBuilder WithValue(string value)
    {
        _sql += " dp.value = @value ";
        _value = value;

        return this;
    }

    public PropertyQueryBuilder WithDescription(string description)
    {
        _sql += " dp.description LIKE '%' + @description + '%' ";
        _description = description;

        return this;
    }

    public CommandDefinition Build()
    {
        return new CommandDefinition(_sql, parameters: new
        {
            device_id = device_id,
            id = _id,
            name = _name,
            value = _value,
            description = _description
        }, cancellationToken: _cancellationToken, transaction: _transaction);
    }

    internal PropertyQueryBuilder WithCriteria(Criteria<Property>? criteria)
    {
        if (criteria is null)
            return this;

        if (criteria.Entity is not null)
        {
            if (string.IsNullOrWhiteSpace(criteria.Entity.Name) is false)
                WithName(criteria.Entity.Name);
            if (string.IsNullOrWhiteSpace(criteria.Entity.Value) is false)
                WithValue(criteria.Entity.Value);
            if (string.IsNullOrWhiteSpace(criteria.Entity.Description) is false)
                WithDescription(criteria.Entity.Description);
        }

        return this;
    }

    internal PropertyQueryBuilder WithId(int id)
    {
        _sql = "dp.id = @id";
        _id = id;

        return this;
    }
}
