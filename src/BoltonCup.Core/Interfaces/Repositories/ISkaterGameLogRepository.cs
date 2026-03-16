namespace BoltonCup.Core;

public interface ISkaterGameLogRepository
{
    Task<IPagedList<SkaterGameLog>> GetAllAsync(GetSkaterGameLogsQuery query);
}
