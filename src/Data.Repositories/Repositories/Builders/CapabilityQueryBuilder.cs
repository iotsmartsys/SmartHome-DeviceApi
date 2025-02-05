using System.Data;
using Dapper;

namespace Data.Repositories;

internal abstract class CapabilityQueryBuilder(string sql) : ICapabilityQueryBuilder
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

    public abstract CommandDefinition Build();
}