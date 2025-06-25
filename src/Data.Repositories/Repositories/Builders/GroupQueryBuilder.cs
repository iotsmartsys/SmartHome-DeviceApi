using System.Data;
using Core.Entities;
using Dapper;

namespace Data.Repositories;

internal class GroupQueryBuilder
{
    private string _sql = GroupQuery.GetAllGroups;
    private int _id;
    private string? _name;
    private bool? _isActive;
    private IconGroup? _icon;
    private CancellationToken _cancellationToken = default;
    private IDbTransaction? _transaction;

    public GroupQueryBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        return this;
    }

    public GroupQueryBuilder WithTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        return this;
    }

    public GroupQueryBuilder WithId(int id)
    {
        _id = id;
        _sql += " AND g.id = @id ";
        return this;
    }

    public GroupQueryBuilder WithName(string name)
    {
        _name = name;
        _sql += " AND g.name = @name ";
        return this;
    }

    public GroupQueryBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        _sql += " AND g.activated = @isActive ";
        return this;
    }

    public GroupQueryBuilder WithIcon(IconGroup icon)
    {
        _icon = icon;
        _sql += " AND g.iconName = @iconName ";
        return this;
    }

    public CommandDefinition Build()
    {
        return new CommandDefinition(_sql, parameters: new
        {
            id = _id,
            name = _name,
            isActive = _isActive,
            iconName = _icon?.Name
        }, cancellationToken: _cancellationToken, transaction: _transaction);
    }
}