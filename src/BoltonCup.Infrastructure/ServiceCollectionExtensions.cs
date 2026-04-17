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
            .AddDbContextFactory<AuthDbContext>(options => options.UseNpgsql(connectionString))
            .AddTransient<IAccountRepository, AccountRepository>()
            .AddTransient<IGameRepository, GameRepository>()
            .AddTransient<IGoalieStatRepository, GoalieStatRepository>()
            .AddTransient<IInfoGuideRepository, InfoGuideRepository>()
            .AddTransient<IPlayerRepository, PlayerRepository>()
            .AddTransient<ISkaterStatRepository, SkaterStatRepository>()
            .AddTransient<ITeamRepository, TeamRepository>()
            .AddTransient<ITournamentRepository, TournamentRepository>()
            .AddTransient<IAccountService, AccountService>()
            .AddTransient<ITeamService, TeamService>()
            .AddTransient<ITournamentService, TournamentService>()
            .AddTransient<ITournamentRegistrationService, TournamentRegistrationService>()
            .AddTransient<IUserService, UserService>()
            .AddTransient<Core.BracketChallenge.IBracketChallengeService, BracketChallengeService>();
        return builder;
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
        
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
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