using BoltonCup.Common.Auth;
using BoltonCup.Common.Handlers;
using BoltonCup.Common.Services;
using BoltonCup.Common.Theme;
using BoltonCup.Core;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        
        // theming
        services.AddSingleton<BoltonCupTheme>();
        
        // s3
        services
            .AddSingleton<IS3UrlResolver, S3UrlResolver>(_ => new S3UrlResolver(bcConfig))
            .AddSingleton<IS3UploadService, S3UploadService>()
            .TryAddSingleton<IAssetUploadService, WasmAssetUploadService>();
        
        // auth
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