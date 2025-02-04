using System.Data;
using Dapper;

namespace Data.Repositories;

internal interface ICapabilityQueryBuilder
{
    ICapabilityQueryBuilder WithCancellationToken(CancellationToken cancellationToken);
    ICapabilityQueryBuilder WithTransaction(IDbTransaction transaction);
    CommandDefinition Build();
}

