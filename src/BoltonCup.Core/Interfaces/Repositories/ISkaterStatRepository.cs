using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface ISkaterStatRepository : IReadOnlyRepository<SkaterStat, GetSkaterStatsQuery, int>
{
}
