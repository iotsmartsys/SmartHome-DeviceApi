using System.Data;
using Core.Entities;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class CapabilityTypeRepository : ICapabilityTypeRepository, IRepository
{
    private readonly ILogger<CapabilityTypeRepository> logger;
    private readonly IDbConnection connection;

    public CapabilityTypeRepository(ILogger<CapabilityTypeRepository> logger, IDbConnection connection)
    {
        this.logger = logger;
        this.connection = connection;
    }

    public async Task CreateAsync(CapabilityType capabilityType)
    {
        const string sql = @"
        INSERT INTO CapabilityTypes (Name, ActuatorMode,DataType, DynamicComputedValue)
        VALUES (@Name, @ActuatorMode, @DataType, @ComputedValue);

        SELECT LAST_INSERT_ID() AS NewId;
        ";

        var newId = await connection.ExecuteScalarAsync<int>(sql, capabilityType);
        capabilityType.Id = newId;
    }

    public async Task<IEnumerable<CapabilityType>> GetAllAsync(string? name)
    {
        const string sql = @"
            SELECT 
                Id, 
                Name, 
                ActuatorMode,
                DataType,
                DynamicComputedValue
            FROM CapabilityTypes
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
                ActuatorMode,
                DataType,
                DynamicComputedValue
            FROM CapabilityTypes
            WHERE Name = @name
        ";

        return await connection.QueryFirstOrDefaultAsync<CapabilityType>(new CommandDefinition( sql, new { name }, cancellationToken: default));
    }
}