using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface IGoalieGameLogRepository : IReadOnlyRepository<GoalieGameLog, GetGoalieGameLogsQuery, int>
{
}
