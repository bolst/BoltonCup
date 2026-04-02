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
using Microsoft.Extensions.Logging;

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
            .AddSingleton<IAssetUrlResolver, AssetUrlResolver>(_ => new AssetUrlResolver(bcConfig))
            .AddSingleton<IAssetFileUploader, AssetFileUploader>()
            .TryAddSingleton<IStorageService, ClientStorageService>();
        
        // auth
        services
            .AddAuthorizationCore()
            .AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>()
            .AddTransient<CookieHandler>()
            .AddTransient<SentryHttpMessageHandler>()
            .AddHttpClient<IBoltonCupApi, BoltonCupApi>(client => 
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<SentryHttpMessageHandler>()
            .AddTypedClient<IBoltonCupApi>((http, sp) => new BoltonCupApi(apiBaseUrl, http));
        
        return services;
    }


    public static ILoggingBuilder AddBoltonCupSentry(this ILoggingBuilder logging, IConfiguration configuration)
    {
        return logging.AddSentry(options =>
        {
            options.Dsn = configuration["Sentry::Dsn"] ?? string.Empty;
            options.TracesSampleRate = 1.0;
            if (configuration["BoltonCup::ApiBaseUrl"] is { } apiBaseUrl)
                options.TracePropagationTargets.Add(apiBaseUrl);
        });
    }
}