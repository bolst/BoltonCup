namespace BoltonCup.Core;

public interface ITeamRepository
{
    Task<IPagedList<Team>> GetAllAsync(GetTeamsQuery query);
    Task<Team?> GetByIdAsync(int id);
    Task<bool> AddAsync(Team entity);
    Task<bool> UpdateAsync(Team entity);
    Task<bool> DeleteAsync(int id);
}