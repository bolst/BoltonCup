using BoltonCup.WebAPI.Models.Base;

namespace BoltonCup.WebAPI.Interfaces;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync(int tournamentId);
}