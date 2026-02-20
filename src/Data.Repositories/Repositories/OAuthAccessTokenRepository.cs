using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class OAuthAccessTokenRepository(ILogger<OAuthAccessTokenRepository> logger, IDbConnection connection)
    : IOAuthAccessTokenRepository, IRepository
{
    public async Task<long> InsertAsync(OAuthAccessToken token, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Inserindo access token para o client {clientId}", token.ClientId);
            const string sql = @"
                INSERT INTO OAuthAccessTokens (AccessToken, UserId, ClientId, Scope, ExpiresAt)
                VALUES (@AccessToken, @UserId, @ClientId, @Scope, @ExpiresAt);
                SELECT LAST_INSERT_ID() AS NewId;";

            var command = new CommandDefinition(sql, token, cancellationToken: cancellationToken);
            var id = await connection.ExecuteScalarAsync<long>(command);
            logger.LogInformation("Access token inserido com sucesso. Id: {id}", id);
            return id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inserir access token para o client {clientId}", token.ClientId);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task<OAuthAccessToken?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Buscando access token");
            const string sql = @"
                SELECT
                    Id,
                    AccessToken,
                    UserId,
                    ClientId,
                    Scope,
                    ExpiresAt,
                    RevokedAt,
                    CreatedAt
                FROM OAuthAccessTokens
                WHERE AccessToken = @AccessToken
                LIMIT 1;";

            var command = new CommandDefinition(sql, new { AccessToken = accessToken }, cancellationToken: cancellationToken);
            return await connection.QuerySingleOrDefaultAsync<OAuthAccessToken>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar access token");
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task RevokeAsync(string accessToken, DateTime revokedAt, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Revogando access token");
            const string sql = @"
                UPDATE OAuthAccessTokens
                SET RevokedAt = @RevokedAt
                WHERE AccessToken = @AccessToken AND RevokedAt IS NULL;";

            var command = new CommandDefinition(sql, new { AccessToken = accessToken, RevokedAt = revokedAt }, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao revogar access token");
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
