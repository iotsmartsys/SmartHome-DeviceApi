using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class OAuthClientRepository(ILogger<OAuthClientRepository> logger, IDbConnection connection)
    : IOAuthClientRepository, IRepository
{
    public async Task<long> InsertAsync(OAuthClient client, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Inserindo OAuth client {clientId}", client.ClientId);
            const string sql = @"
                INSERT INTO OAuthClients (ClientId, ClientSecret, Name, IsActive)
                VALUES (@ClientId, @ClientSecret, @Name, @IsActive);
                SELECT LAST_INSERT_ID() AS NewId;";

            var command = new CommandDefinition(sql, client, cancellationToken: cancellationToken);
            var id = await connection.ExecuteScalarAsync<long>(command);
            logger.LogInformation("OAuth client inserido com sucesso. Id: {id}", id);
            return id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inserir OAuth client {clientId}", client.ClientId);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task<OAuthClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Buscando OAuth client {clientId}", clientId);
            const string sql = @"
                SELECT
                    Id,
                    ClientId,
                    ClientSecret,
                    Name,
                    IsActive,
                    CreatedAt
                FROM OAuthClients
                WHERE ClientId = @ClientId
                LIMIT 1;";

            var command = new CommandDefinition(sql, new { ClientId = clientId }, cancellationToken: cancellationToken);
            return await connection.QuerySingleOrDefaultAsync<OAuthClient>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar OAuth client {clientId}", clientId);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
