namespace BoltonCup.Core;

public interface ITournamentRepository
{
    Task<IPagedList<Tournament>> GetAllAsync(GetTournamentsQuery query, CancellationToken cancellationToken = default);
    Task<Tournament?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}