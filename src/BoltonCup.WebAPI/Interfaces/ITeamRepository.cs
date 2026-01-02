using BoltonCup.WebAPI.Data.Entities;

namespace BoltonCup.WebAPI.Interfaces;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync(int tournamentId);
}