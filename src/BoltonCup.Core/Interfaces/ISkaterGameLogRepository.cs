using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface ISkaterGameLogRepository : IReadOnlyRepository<SkaterGameLog, GetSkaterGameLogsQuery, int>
{
}
