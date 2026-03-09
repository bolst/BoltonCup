using Amazon.S3;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
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
            .AddTransient<ITournamentRepository, TournamentRepository>();
    }
    
    public static IServiceCollection AddBoltonCupS3(this IServiceCollection services, IConfiguration configuration)
    {
        var r2Config = configuration.GetRequiredSection("CloudflareR2");
        var accountId = r2Config["AccountId"];
        var accessKey = r2Config["AccessKey"];
        var secretKey = r2Config["SecretKey"];
        
        var s3Credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            AuthenticationRegion = "auto"
        };
        return services
            .AddSingleton<IAmazonS3>(_ => new AmazonS3Client(s3Credentials, s3Config))
            .AddSingleton<IAssetUploadService, AssetUploadService>();
    }
}

public static class ConfigurationPaths
{
    public const string ConnectionString = "BoltonCup:ConnectionString";
}