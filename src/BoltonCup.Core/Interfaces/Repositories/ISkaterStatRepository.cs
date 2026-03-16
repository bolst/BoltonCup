namespace BoltonCup.Core;

public interface ISkaterStatRepository
{
    Task<IPagedList<SkaterStat>> GetAllAsync(GetSkaterStatsQuery query);
}
