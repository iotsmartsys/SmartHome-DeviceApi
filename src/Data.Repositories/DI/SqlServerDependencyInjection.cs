
using System.Data;
using Core.Contracts.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Repositories.SqlServer.DI;

public static class SqlServerDependencyInjection
{
    public static IServiceCollection AddSqlServerData(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));
        services
            .AddScoped<IDeviceRepository, DeviceRepository>()
            .AddScoped<IDeviceCapabilityRepository, DeviceCapabilityRepository>()
            .AddScoped<ICapabilityTypeRepository, CapabilityTypeRepository>()
            .AddScoped<IPlatformRepository, PlatformRepository>();

        return services;
    }
}