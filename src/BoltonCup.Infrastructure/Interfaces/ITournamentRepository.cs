using BoltonCup.Infrastructure.Data.Entities;

namespace BoltonCup.Infrastructure.Interfaces;

public interface ITournamentRepository : IRepository<Tournament, int>
{
    Task<Tournament?> GetActiveAsync();
}