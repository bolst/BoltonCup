namespace BoltonCup.Core;

public interface IInfoGuideRepository
{
    Task<IPagedList<InfoGuide>> GetAllAsync(GetInfoGuidesQuery query, CancellationToken cancellationToken = default);
    Task<InfoGuide?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InfoGuide?> GetByTournamentIdAsync(int tournamentId, CancellationToken cancellationToken = default);
}