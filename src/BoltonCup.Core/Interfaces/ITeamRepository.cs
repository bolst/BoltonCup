namespace BoltonCup.Core;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync();
    Task<IEnumerable<Team>> GetAllAsync(int tournamentId);
}