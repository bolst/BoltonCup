using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Interfaces;
using BoltonCup.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoltonCup.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddBoltonCupServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>(ConfigurationPaths.ConnectionString);

        services
            .AddIdentityApiEndpoints<IdentityUser>()
            .AddEntityFrameworkStores<AuthDbContext>();
        
        return services
            .AddDbContext<BoltonCupDbContext>(options => options.UseNpgsql(connectionString))
            .AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString))
            .AddTransient<ITeamRepository, TeamRepository>()
            .AddTransient<ITournamentRepository, TournamentRepository>()
            .AddTransient<IPlayerRepository, PlayerRepository>();
    }   
}