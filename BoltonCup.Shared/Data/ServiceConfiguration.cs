using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Shared.Data;

public static class ServiceConfiguration
{
    public static IServiceCollection AddBoltonCupServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<IBCData>(sp =>
        {
            var connectionString = configuration["SB_CSTRING"];
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