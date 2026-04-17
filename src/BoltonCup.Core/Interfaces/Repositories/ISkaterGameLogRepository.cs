namespace BoltonCup.Core;

public interface ISkaterGameLogRepository
{
    Task<IPagedList<SkaterGameLog>> GetAllAsync(GetSkaterGameLogsQuery query, CancellationToken cancellationToken = default);
}
