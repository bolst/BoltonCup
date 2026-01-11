using BoltonCup.Core.Base;

namespace BoltonCup.Core;

public interface IGoalieStatRepository : IReadOnlyRepository<GoalieStat, GetGoalieStatsQuery, int>
{
}
