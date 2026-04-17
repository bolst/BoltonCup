namespace BoltonCup.Core;

public interface ITeamRepository
{
    Task<IPagedList<Team>> GetAllAsync(GetTeamsQuery query, CancellationToken cancellationToken = default);
    Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}