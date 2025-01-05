using System.Data;
using Core.Entities;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.SqlServer.Repositories;

internal class CapabilityTypeRepository : ICapabilityTypeRepository, IRepository
{
    private readonly ILogger<CapabilityTypeRepository> logger;
    private readonly IDbConnection connection;

    public CapabilityTypeRepository(ILogger<CapabilityTypeRepository> logger, IDbConnection connection)
    {
        this.logger = logger;
        this.connection = connection;
    }

    public async Task<IEnumerable<CapabilityType>> GetAllAsync(string? name)
    {
        const string sql = @"
            SELECT 
                Id, 
                Name, 
                ActuatorMode
            FROM Capabilities
            WHERE Name LIKE @name
            ORDER BY Name
        ";

        return await connection.QueryAsync<CapabilityType>(sql, new { name = $"%{name}%" });
    }

    public async Task<CapabilityType?> GetByNameAsync(string name)
    {
        const string sql = @"
            SELECT 
                Id, 
                Name, 
                ActuatorMode
            FROM Capabilities
            WHERE Name = @name
        ";

        return await connection.QueryFirstOrDefaultAsync<CapabilityType>(sql, new { name });
    }
}