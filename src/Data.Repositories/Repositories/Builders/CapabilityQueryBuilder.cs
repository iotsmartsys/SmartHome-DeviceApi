using System.Data;
using Dapper;

namespace Data.Repositories;

internal class CapabilityQueryBuilder(string sql = CapabilityQuery.GetAllCapabilities) : ICapabilityQueryBuilder
{
    protected string _sql = sql;
    protected CancellationToken cancellationToken = default;
    protected IDbTransaction? _transaction;

    public ICapabilityQueryBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        this.cancellationToken = cancellationToken;
        return this;
    }

    public ICapabilityQueryBuilder WithTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        return this;
    }

    public virtual CommandDefinition Build()
    {
        return new CommandDefinition(_sql, cancellationToken: cancellationToken, transaction: _transaction);
    }
}