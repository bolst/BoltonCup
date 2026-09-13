using BoltonCup.Core;
using BoltonCup.Application.Services;
using BoltonCup.Application.Settings;
using BoltonCup.Integrations;
using BoltonCup.Persistence;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Application;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddBoltonCupApplication(this WebApplicationBuilder builder)
    {
        builder.AddBoltonCupPersistence();
        builder.AddBoltonCupIntegrations();

        builder.Services.AddMemoryCache();

        RegisterByConvention(builder.Services, typeof(AccountService).Assembly, "Service");

        // Convention scanning matches on Type.Name, which for a generic is "TagService`1" and so
        // never ends with "Service". Open generics must be registered explicitly.
        builder.Services.AddTransient(typeof(ITagService<>), typeof(TagService<>));

        builder.Services.AddSingleton<IRosterValidator, RosterValidator>();
        builder.Services.AddHostedService<StatisticsRefreshBackgroundService>();

        // The shared music rotation is DB-backed; register explicitly (name ends in Queue, not Service).
        builder.Services.AddScoped<IGlobalMusicQueue, GlobalMusicQueue>();

        builder.Services.Configure<TradeNotificationSettings>(builder.Configuration.GetSection("TradeNotifications"));

        return builder;
    }

    // Registers all concrete classes ending with `suffix` against their matching interface (I<ClassName>).
    // Skips classes with no matching interface.
    static void RegisterByConvention(IServiceCollection services, System.Reflection.Assembly implAssembly, string suffix)
    {
        var concreteTypes = implAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith(suffix));

        foreach (var impl in concreteTypes)
        {
            var interfaceName = $"I{impl.Name}";
            var interfaces = impl.GetInterfaces();
            if (interfaces.FirstOrDefault(i => i.Name == interfaceName) is { } serviceType)
            {
                services.AddTransient(serviceType, impl);
            }
        }
    }

    public static WebApplicationBuilder AddBoltonCupAssetUrlResolver(this WebApplicationBuilder builder)
    {
        var r2Config = builder.Configuration.GetRequiredSection("CloudflareR2");
        var baseUrl = r2Config["BaseUrl"];
        builder.Services.AddSingleton<IAssetUrlResolver, AssetUrlResolver>(_ => new AssetUrlResolver(baseUrl!));
        return builder;
    }
}