using BoltonCup.Core;

namespace BoltonCup.Core;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync(int tournamentId);
}