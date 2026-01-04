using BoltonCup.Core;

namespace BoltonCup.Core;

public interface ITournamentRepository : IRepository<Tournament, int>
{
    Task<Tournament?> GetActiveAsync();
}