using Core.Contracts.Repositories;

namespace Data.Repositories;

internal static class CapabilityQueryBuilderFactory
{
    public static IFindCapabilityQueryBuilder Create(CapabilityFind capabilityQuery)
    {
        return new FindCapabilityQueryBuilder(capabilityQuery);
    }
}

