using BoltonCup.Core.Base;

namespace BoltonCup.Core;

public interface IGameRepository : IRepository<Game, GetGamesQuery, int>
{
}
