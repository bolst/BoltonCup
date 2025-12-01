using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Repositories;

namespace BoltonCup.WebAPI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoltonCupWebAPIServices(this IServiceCollection services)
    {
        return services
            .AddTransient<ITeamRepository, TeamRepository>()
            .AddTransient<IUnitOfWork, UnitOfWork>();
    }   
}