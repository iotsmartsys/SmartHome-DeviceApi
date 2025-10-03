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
        try
        {
            return await connection.QueryAsync<Platform>(sql);
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
internal class MonitoredPlaceRepository(IDbConnection connection) : IMonitoredPlaceRepository
{
    public async Task<IEnumerable<MonitoredPlace>> GetAllAsync()
    {
        try
        {
            return await connection.QueryAsync<MonitoredPlace>(MonitoredPlaceQuery.GetAll);
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}