namespace BoltonCup.Core;

public interface ITournamentService
{
    Task<IPagedList<Tournament>> GetAllAsync(GetTournamentsQuery query, CancellationToken cancellationToken = default);
    Task<Tournament?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Tournament?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Tournament?> GetFeaturedStatsAsync(CancellationToken cancellationToken = default);
    Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBackgroundImageAsync(int tournamentId, string tempKey, CancellationToken cancellationToken = default);
}