namespace BoltonCup.Core;

public interface IGoalieStatRepository
{
    Task<IPagedList<GoalieStat>> GetAllAsync(GetGoalieStatsQuery query);
}
