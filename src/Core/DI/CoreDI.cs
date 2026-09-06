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
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDashboardWidgetCompatibilityResolver, DashboardWidgetCompatibilityResolver>();
        return services;
    }
}
