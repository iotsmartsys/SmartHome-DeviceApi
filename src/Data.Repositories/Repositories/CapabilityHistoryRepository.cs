using System.Data;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class CapabilityHistoryRepository(ILogger<CapabilityHistoryRepository> logger, IDbConnection connection) : ICapabilityHistoryRepository, IRepository
{
    public async Task<IEnumerable<CapabilityHistory>> GetByCapabilityIdAsync(int capabilityId, CapabilityHistoryFind? historyFind, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando histórico de capabilities para a capability {capabilityId}", capabilityId);
        const string sql = CapabilityQuery.SelectHistory;
        var command = new CommandDefinition(sql, new
        {
            CapabilityId = capabilityId,
            LastHours = historyFind?.last_hours,
            DateStart = historyFind?.date_start,
            DateEnd = historyFind?.date_end
        }, cancellationToken: cancellationToken);

        var history = await connection.QueryAsync<CapabilityHistory>(command);
        if (!history.Any())
        {
            logger.LogWarning("Nenhum histórico encontrado para a capability {capabilityId}", capabilityId);
            return [];
        }

        logger.LogInformation("Histórico de capabilities encontrado para a capability {capabilityId}", capabilityId);
        return history;
    }

    public async Task AddAsync(string capability_name, string value, CancellationToken cancellationToken)
    {
        logger.LogInformation("Inserindo histórico para a capability {capability_name}", capability_name);
        const string sql = CapabilityQuery.InsertHistory;
        var command = new CommandDefinition(sql, new
        {
            CapabilityName = capability_name,
            Value = value
        }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
        logger.LogInformation("Histórico inserido para a capability {capability_name}", capability_name);
    }
}
