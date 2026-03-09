using Amazon.S3;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Interfaces;
using BoltonCup.Infrastructure.Repositories;
using BoltonCup.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoltonCupInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>();

        services.AddBoltonCupS3Services(configuration);
        
        var connectionString = configuration.GetValue<string>(ConfigurationPaths.ConnectionString);
        return services
            .AddDbContextFactory<BoltonCupDbContext>(options => options.UseNpgsql(connectionString))
            .AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString))
            .AddTransient<IAccountRepository, AccountRepository>()
            .AddTransient<IGameRepository, GameRepository>()
            .AddTransient<IGoalieGameLogRepository, GoalieGameLogRepository>()
            .AddTransient<IGoalieStatRepository, GoalieStatRepository>()
            .AddTransient<IInfoGuideRepository, InfoGuideRepository>()
            .AddTransient<IPlayerRepository, PlayerRepository>()
            .AddTransient<ISkaterGameLogRepository, SkaterGameLogRepository>()
            .AddTransient<ISkaterStatRepository, SkaterStatRepository>()
            .AddTransient<ITeamRepository, TeamRepository>()
            .AddTransient<ITournamentRepository, TournamentRepository>()
            .AddTransient<IAssetUploadService, AssetUploadService>();
    }

    private static IServiceCollection AddBoltonCupS3Services(this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection("CloudflareR2");
        var accountId = options.GetValue<string>("AccountId");
        var accessKey = options.GetValue<string>("AccessKey");
        var secretKey = options.GetValue<string>("SecretKey");
        
        var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            AuthenticationRegion = "auto"
        };
        return services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(credentials, config));
    }
}

public static class ConfigurationPaths
{
    public const string ConnectionString = "BoltonCup:ConnectionString";
}