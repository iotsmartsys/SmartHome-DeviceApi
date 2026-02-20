using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IOAuthAccessTokenRepository : IRepository
{
    Task<long> InsertAsync(OAuthAccessToken token, CancellationToken cancellationToken);
    Task<OAuthAccessToken?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken);
    Task RevokeAsync(string accessToken, DateTime revokedAt, CancellationToken cancellationToken);
}
