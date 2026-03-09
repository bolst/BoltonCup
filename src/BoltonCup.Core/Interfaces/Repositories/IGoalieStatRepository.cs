using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface IGoalieStatRepository : IReadOnlyRepository<GoalieStat, GetGoalieStatsQuery, int>
{
}
