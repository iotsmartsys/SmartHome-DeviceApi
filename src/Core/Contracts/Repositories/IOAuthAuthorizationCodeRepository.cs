using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IOAuthAuthorizationCodeRepository : IRepository
{
    Task<long> InsertAsync(OAuthAuthorizationCode code, CancellationToken cancellationToken);
    Task<OAuthAuthorizationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task MarkUsedAsync(string code, DateTime usedAt, CancellationToken cancellationToken);
}
