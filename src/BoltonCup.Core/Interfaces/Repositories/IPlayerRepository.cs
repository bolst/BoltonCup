using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface IPlayerRepository : IRepository<Player, GetPlayersQuery, int>
{
}
