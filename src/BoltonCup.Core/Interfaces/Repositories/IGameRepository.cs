namespace BoltonCup.Core;

public interface IGameRepository
{
    Task<IPagedList<Game>> GetAllAsync(GetGamesQuery query, CancellationToken cancellationToken = default);
    Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
