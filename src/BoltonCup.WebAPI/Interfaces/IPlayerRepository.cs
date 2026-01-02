using BoltonCup.WebAPI.Data.Entities;

namespace BoltonCup.WebAPI.Interfaces;

public interface IPlayerRepository : IRepository<Player, Guid>
{
    Task<IEnumerable<Player>> GetAllAsync(int tournamentId);
}