using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Shared.Data;

public static class ServiceConfiguration
{
    public static IServiceCollection AddBoltonCupServices(
        this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<IBCData>(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("SB_CSTRING");
            var cacheService = sp.GetRequiredService<ICacheService>();
            return new BCData(connectionString!, cacheService);
        });

        return services;
    }
}