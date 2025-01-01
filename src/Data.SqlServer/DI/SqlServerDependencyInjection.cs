using System;
using System.Data;
using Core.Contracts.Repositories;
using Data.SqlServer.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Data.SqlServer.DI;

public static class SqlServerDependencyInjection
{
    public static IServiceCollection AddSqlServerData(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        return services;
    }
}
