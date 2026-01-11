using BoltonCup.Core.Base;

namespace BoltonCup.Core;

public interface ISkaterStatRepository : IReadOnlyRepository<SkaterStat, GetSkaterStatsQuery, int>
{
}
