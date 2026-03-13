namespace BoltonCup.Core;

public interface IGoalieGameLogRepository
{
    Task<IPagedList<GoalieGameLog>> GetAllAsync(GetGoalieGameLogsQuery query);
}
