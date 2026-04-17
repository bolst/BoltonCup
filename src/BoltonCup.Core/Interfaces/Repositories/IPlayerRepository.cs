namespace BoltonCup.Core;

public interface IPlayerRepository
{
    Task<IPagedList<Player>> GetAllAsync(GetPlayersQuery query, CancellationToken cancellationToken = default);
    Task<Player?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
