using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class OAuthAuthorizationCodeRepository(ILogger<OAuthAuthorizationCodeRepository> logger, IDbConnection connection)
    : IOAuthAuthorizationCodeRepository, IRepository
{
    public async Task<long> InsertAsync(OAuthAuthorizationCode code, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Inserindo authorization code para o client {clientId}", code.ClientId);
            const string sql = @"
                INSERT INTO OAuthAuthorizationCodes (Code, ClientId, RedirectUri, UserId, Scope, ExpiresAt)
                SELECT @Code, @ClientId, @RedirectUri, @UserId, @Scope, @ExpiresAt
                WHERE NOT EXISTS (
                    SELECT 1 FROM OAuthAuthorizationCodes WHERE Code = @Code
                );
                SELECT LAST_INSERT_ID() AS NewId;";

            var command = new CommandDefinition(sql, code, cancellationToken: cancellationToken);
            var id = await connection.ExecuteScalarAsync<long?>(command) ?? 0;
            logger.LogInformation("Authorization code inserido com sucesso. Id: {id}", id);
            return id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inserir authorization code para o client {clientId}", code.ClientId);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task<OAuthAuthorizationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Buscando authorization code {code}", code);
            const string sql = @"
                SELECT
                    Id,
                    Code,
                    ClientId,
                    RedirectUri,
                    UserId,
                    Scope,
                    ExpiresAt,
                    UsedAt,
                    CreatedAt
                FROM OAuthAuthorizationCodes
                WHERE Code = @Code
                LIMIT 1;";

            var command = new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken);
            return await connection.QuerySingleOrDefaultAsync<OAuthAuthorizationCode>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar authorization code {code}", code);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task MarkUsedAsync(string code, DateTime usedAt, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Marcando authorization code {code} como usado", code);
            const string sql = @"
                UPDATE OAuthAuthorizationCodes
                SET UsedAt = @UsedAt
                WHERE Code = @Code AND UsedAt IS NULL;";

            var command = new CommandDefinition(sql, new { Code = code, UsedAt = usedAt }, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao marcar authorization code {code} como usado", code);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
