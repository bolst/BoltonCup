using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoltonCupInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>(ConfigurationPaths.ConnectionString);

        services
            .AddIdentityApiEndpoints<IdentityUser>()
            .AddEntityFrameworkStores<AuthDbContext>();
        
        return services
            .AddDbContext<BoltonCupDbContext>(options => options.UseNpgsql(connectionString))
            .AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString))
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
}

public static class ConfigurationPaths
{
    public const string ConnectionString = "BoltonCup:ConnectionString";
}