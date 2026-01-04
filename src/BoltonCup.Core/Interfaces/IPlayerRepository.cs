namespace BoltonCup.Core;

public interface IPlayerRepository : IRepository<Player, int>
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<IEnumerable<Player>> GetAllAsync(int tournamentId);
}
