
using System.Data;
using Core.Contracts.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace Data.Repositories.MySql.DI;

public static class MySqlDependencyInjection
{
    public static IServiceCollection AddMySqlData(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A ConnectionString deve ser informada.");

        services.AddScoped<IDbConnection>(_ => new MySqlConnection(connectionString));
        services
            .AddScoped<IDeviceRepository, DeviceRepository>()
            .AddScoped<ICapabilityRepository, CapabilityRepository>()
            .AddScoped<ICapabilityTypeRepository, CapabilityTypeRepository>()
            .AddScoped<IPlatformRepository, PlatformRepository>()
            .AddScoped<IPropertyRepository, PropertyRepository>()
            .AddScoped<IGroupRepository, GroupRepository>()
            ;

        return services;
    }
}