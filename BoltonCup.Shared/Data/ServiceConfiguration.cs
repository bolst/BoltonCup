using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace BoltonCup.Shared.Data;

public static class ServiceConfiguration
{
    public static IServiceCollection AddBoltonCupServices(
        this IServiceCollection services)
    {
        services.AddMemoryCache();
        
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<RegistrationStateService>();

        services.AddScoped<IBCData>(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("SB_CSTRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("SET YOUR ENV VARIABLES!\n");
            }
            var cacheService = sp.GetRequiredService<ICacheService>();
            return new BCData(connectionString!, cacheService);
        });

        return services;
    }
}