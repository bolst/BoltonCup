using BoltonCup.Infrastructure.Data.Entities;

namespace BoltonCup.Infrastructure.Interfaces;

public interface IPlayerRepository : IRepository<Player, int>
{
    Task<IEnumerable<Player>> GetAllAsync(int tournamentId);
}
