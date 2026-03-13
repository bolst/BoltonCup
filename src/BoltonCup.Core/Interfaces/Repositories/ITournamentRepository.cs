namespace BoltonCup.Core;

public interface ITournamentRepository
{
    Task<IPagedList<Tournament>> GetAllAsync(GetTournamentsQuery query);
    Task<Tournament?> GetByIdAsync(int id);
    Task<Tournament?> GetActiveAsync();
    Task<bool> AddAsync(Tournament entity);
    Task<bool> UpdateAsync(Tournament entity);
    Task<bool> DeleteAsync(int id);
}