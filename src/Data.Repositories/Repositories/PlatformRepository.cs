using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;

namespace Data.Repositories;

internal class PlatformRepository(IDbConnection connection) : IPlatformRepository
{
    public async Task<IEnumerable<Platform>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name, Description FROM Platforms";
        return await connection.QueryAsync<Platform>(sql);
    }
}
internal class MonitoredPlaceRepository(IDbConnection connection) : IMonitoredPlaceRepository
{
    public async Task<IEnumerable<MonitoredPlace>> GetAllAsync()
    {
        return await connection.QueryAsync<MonitoredPlace>(MonitoredPlaceQuery.GetAll);
    }
}