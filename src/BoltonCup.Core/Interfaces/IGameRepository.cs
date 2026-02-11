using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface IGameRepository : IRepository<Game, GetGamesQuery, int>
{
}
