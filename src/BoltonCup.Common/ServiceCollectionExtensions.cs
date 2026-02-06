using BoltonCup.Common.Auth;
using BoltonCup.Common.Handlers;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Common;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Adds Bolton Cup authentication.
    /// Looks for the base URL of the Bolton Cup API in the configuration under "BoltonCupApi:BaseUrl".
    /// If not found, an exception is thrown.
    /// </summary>
    public static IServiceCollection AddBoltonCupAuth(this IServiceCollection services, IConfiguration configuration)
    {
        return AddBoltonCupAuth(services, configuration["BoltonCupApi:BaseUrl"]);
    }
    
    public static IServiceCollection AddBoltonCupAuth(this IServiceCollection services, string? apiBaseUrl)
    {
        if (string.IsNullOrEmpty(apiBaseUrl)) 
            throw new ArgumentException("API base URL must be provided.", nameof(apiBaseUrl));
        
        services
            .AddAuthorizationCore()
            .AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>()
            .AddTransient<CookieHandler>()
            .AddHttpClient<IBoltonCupApi, BoltonCupApi>(client => 
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<CookieHandler>()
            .AddTypedClient<IBoltonCupApi>((http, sp) => new BoltonCupApi(apiBaseUrl, http));
        return services;
    }
}