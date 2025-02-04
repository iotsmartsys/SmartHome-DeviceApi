using Dapper;

namespace Data.Repositories;

internal class GenericCapabilityQueryBuilder() : CapabilityQueryBuilder(CapabilityQuery.GetCapabilitiesByDeviceAsync)
{
    public override CommandDefinition Build()
    {
        return new(_sql, cancellationToken, _transaction);
    }
}

