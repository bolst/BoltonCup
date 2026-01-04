using BoltonCup.Infrastructure.Data.Entities;

namespace BoltonCup.Infrastructure.Interfaces;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync(int tournamentId);
}