using BoltonCup.WebAPI.Models;

namespace BoltonCup.WebAPI.Interfaces;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync(int tournamentId);
}