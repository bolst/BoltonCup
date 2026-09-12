namespace BoltonCup.Core;

public interface IGoalieStatService
{
    Task<IPagedList<GoalieStat>> GetAllAsync(GetGoalieStatsQuery query, CancellationToken cancellationToken = default);
}