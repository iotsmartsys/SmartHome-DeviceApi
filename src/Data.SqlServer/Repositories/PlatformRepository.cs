using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;

namespace Data.SqlServer.Repositories;

internal class PlatformRepository(IDbConnection connection) : IPlatformRepository
{
    public async Task<IEnumerable<Platform>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name, Description FROM Platforms";
        return await connection.QueryAsync<Platform>(sql);
    }
}