namespace BoltonCup.Core;

public interface IGameRepository
{
    Task<IPagedList<Game>> GetAllAsync(GetGamesQuery query);
    Task<Game?> GetByIdAsync(int id);
    Task<bool> AddAsync(Game entity);
    Task<bool> UpdateAsync(Game entity);
    Task<bool> DeleteAsync(int id);
}
