using BoltonCup.WebAPI.Data.Entities;

namespace BoltonCup.WebAPI.Interfaces;

public interface IPlayerRepository : IRepository<Player, int>
{
    Task<IEnumerable<Player>> GetAllAsync(int tournamentId);
}
