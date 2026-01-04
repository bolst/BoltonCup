namespace BoltonCup.Core;

public interface ITeamRepository : IRepository<Team, int>
{
    Task<IEnumerable<Team>> GetAllAsync(GetTeamsQuery query);
}