namespace BoltonCup.Core;

public interface IPlayerRepository
{
    Task<IPagedList<Player>> GetAllAsync(GetPlayersQuery query);
    Task<Player?> GetByIdAsync(int id);
    Task<bool> AddAsync(Player entity);
    Task<bool> UpdateAsync(Player entity);
    Task<bool> DeleteAsync(int id);
}
