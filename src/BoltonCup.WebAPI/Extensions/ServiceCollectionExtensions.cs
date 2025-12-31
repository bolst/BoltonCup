using BoltonCup.WebAPI.Data;
using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoltonCupWebAPIServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>(ConfigurationPaths.ConnectionString);

        return services
            .AddDbContext<BoltonCupDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            })
            .AddTransient<ITeamRepository, TeamRepository>()
            .AddTransient<ITournamentRepository, TournamentRepository>();
    }   
}