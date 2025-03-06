using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;

namespace BoltonCup.Shared.Data;

public static class ServiceConfiguration
{
    public static IServiceCollection AddBoltonCupServices(
        this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddBlazoredLocalStorage();
        
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<RegistrationStateService>();

        services.AddScoped<IBCData>(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("SB_CSTRING");
            var cacheService = sp.GetRequiredService<ICacheService>();
            return new BCData(connectionString!, cacheService);
        });

        return services;
    }
}