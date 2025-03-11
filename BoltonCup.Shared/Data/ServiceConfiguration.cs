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
        
        services.AddScoped(provider =>
        {
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
            
            return new Supabase.Client(url, key, new SupabaseOptions
            {
                AutoConnectRealtime = true,
            });
        });

        services.AddScoped<IBCData>(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("SB_CSTRING");
            var cacheService = sp.GetRequiredService<ICacheService>();
            return new BCData(connectionString!, cacheService);
        });

        return services;
    }
}