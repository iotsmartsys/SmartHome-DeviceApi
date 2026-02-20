using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IOAuthClientRepository : IRepository
{
    Task<long> InsertAsync(OAuthClient client, CancellationToken cancellationToken);
    Task<OAuthClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken);
}
