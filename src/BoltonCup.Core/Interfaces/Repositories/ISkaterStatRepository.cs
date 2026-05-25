namespace BoltonCup.Core;

public interface ISkaterStatRepository
{
    Task<IPagedList<SkaterStat>> GetAllAsync(GetSkaterStatsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkaterStat>> GetCareerStatsAsync(int tournamentId, int? teamId, CancellationToken cancellationToken = default);
}
