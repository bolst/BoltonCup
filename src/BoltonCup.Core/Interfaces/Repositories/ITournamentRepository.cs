using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface ITournamentRepository : IRepository<Tournament, GetTournamentsQuery, int>
{
    Task<Tournament?> GetActiveAsync();
}