using Amazon.S3;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Repositories;
using BoltonCup.Infrastructure.Services;
using BoltonCup.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RazorLight;

namespace BoltonCup.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddBoltonCupInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentityCore<BoltonCupUser>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<BoltonCupClaimsPrincipalFactory>();

        builder.AddBoltonCupEmails();
        builder.AddBoltonCupS3();
        builder.AddBoltonCupPayments();
        
        var connectionString = builder.Configuration.GetValue<string>(ConfigurationPaths.ConnectionString);
        builder.Services
            .AddDbContextFactory<BoltonCupDbContext>(options => options.UseNpgsql(connectionString))
            .AddDbContextFactory<AuthDbContext>(options => options.UseNpgsql(connectionString));

        RegisterByConvention(builder.Services, typeof(AccountRepository).Assembly, "Repository");
        RegisterByConvention(builder.Services, typeof(AccountRepository).Assembly, "Service");

        return builder;
    }

    // Registers all concrete classes ending with `suffix` against their matching interface (I<ClassName>)
    // found in any loaded assembly. Skips classes with no matching interface.
    private static void RegisterByConvention(IServiceCollection services, System.Reflection.Assembly implAssembly, string suffix)
    {
        var interfaceAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        var concreteTypes = implAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(suffix));

        foreach (var impl in concreteTypes)
        {
            var interfaceName = $"I{impl.Name}";
            var serviceType = interfaceAssemblies
                .SelectMany(a => { try { return a.GetTypes(); } catch { return []; } })
                .FirstOrDefault(t => t.IsInterface && t.Name == interfaceName);

            if (serviceType is not null)
                services.AddTransient(serviceType, impl);
        }
    }

    public static WebApplicationBuilder AddBoltonCupAssetUrlResolver(this WebApplicationBuilder builder)
    {
        var r2Config = builder.Configuration.GetRequiredSection("CloudflareR2");
        var baseUrl = r2Config["BaseUrl"];
        builder.Services.AddSingleton<IAssetUrlResolver, AssetUrlResolver>(_ => new AssetUrlResolver(baseUrl!));
        return builder;
    }

    private static IServiceCollection AddBoltonCupEmails(this WebApplicationBuilder builder)
    {
        var razorEngine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(EmailSender).Assembly, "BoltonCup.Infrastructure.EmailTemplates")
            .UseMemoryCachingProvider()
            .UseOptions(new RazorLightOptions
            {
                EnableDebugMode = !builder.Environment.IsProduction(),
            })
            .Build();

        builder.Services.AddSingleton<IRazorLightEngine>(razorEngine);

        builder.Services.Configure<ResendSettings>(builder.Configuration.GetSection("Resend"));
        builder.Services.AddHttpClient<IEmailTransport, ResendEmailTransport>(client =>
        {
            client.BaseAddress = new Uri("https://api.resend.com/");
        });

        return builder.Services
            .AddSingleton<IEmailQueue, EmailQueue>()
            .AddHostedService<EmailBackgroundService>()
            .AddTransient<IEmailer, EmailSender>();
    }
    
    private static IServiceCollection AddBoltonCupS3(this WebApplicationBuilder builder)
    {
        var r2Config = builder.Configuration.GetRequiredSection("CloudflareR2");
        var accountId = r2Config["AccountId"];
        var accessKey = r2Config["AccessKey"];
        var secretKey = r2Config["SecretKey"];
        
        var s3Credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            AuthenticationRegion = "auto"
        };
        return builder.Services
            .AddSingleton<IAmazonS3>(_ => new AmazonS3Client(s3Credentials, s3Config))
            .AddSingleton<IAssetKeyGenerator, AssetKeyGenerator>()
            .Replace(ServiceDescriptor.Singleton<IStorageService, ServerStorageService>());
    }

    private static IServiceCollection AddBoltonCupPayments(this WebApplicationBuilder builder)
    { 
        Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetRequiredSection("Stripe").GetValue<string>(nameof(StripeSettings.ApiKey));
        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
        return builder.Services.AddTransient<ITournamentPaymentService, TournamentPaymentService>();
    }
}

public static class ConfigurationPaths
{
    public const string ConnectionString = "BoltonCup:ConnectionString";
}