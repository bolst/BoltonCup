using BoltonCup.WebAPI.Models;

namespace BoltonCup.WebAPI.Interfaces;

public interface IPlayerRepository : IRepository<Player, Guid>
{
    Task<IEnumerable<Player>> GetAllAsync(int tournamentId);
}