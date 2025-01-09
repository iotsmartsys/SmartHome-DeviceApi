using System;
using Core.Contracts.Services;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.DI;

public static class CoreDI
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<IAddCapabilityService, AddCapabilityService>();
        return services;
    }
}
