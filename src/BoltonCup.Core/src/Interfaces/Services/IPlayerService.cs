namespace BoltonCup.Core;

public interface IPlayerService
{
    Task<IPagedList<Player>> GetAllAsync(GetPlayersQuery query, CancellationToken cancellationToken = default);
    Task<Player?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}