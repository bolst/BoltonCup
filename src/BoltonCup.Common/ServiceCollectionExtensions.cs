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
    /// Adds common services for Bolton Cup applications.
    /// Requires appsettings with a <see cref="BoltonCupConfiguration"/>.
    /// </summary>
    public static IServiceCollection AddBoltonCupCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        var configSection = configuration.GetSection(BoltonCupConfiguration.SectionName);
        services.Configure<BoltonCupConfiguration>(configSection);
        
        var bcConfig = configSection.Get<BoltonCupConfiguration>();
        var apiBaseUrl = bcConfig?.ApiBaseUrl
            ?? throw new ArgumentException("Missing API base URL.", nameof(BoltonCupConfiguration.ApiBaseUrl));
        
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