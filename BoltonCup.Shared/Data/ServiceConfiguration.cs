using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

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
    
    public static IServiceCollection AddBoltonCupSupabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(provider =>
        {
            var url = configuration["SUPABASE_URL"];
            var key = configuration["SUPABASE_KEY"];

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("SET YOUR ENV VARIABLES!\n");
            }
            
            return new Supabase.Client(url, key, new SupabaseOptions
            {
                AutoConnectRealtime = true,
            });
        });

        return services;
    }
    
    public static IServiceCollection AddBoltonCupAuth(
        this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddScoped<CustomUserService>();
        services.AddScoped<CustomAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

        return services;
    }
    
}