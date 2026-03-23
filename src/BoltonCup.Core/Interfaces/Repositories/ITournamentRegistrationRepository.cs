namespace BoltonCup.Core;

public interface ITournamentRegistrationRepository
{
    Task<TournamentRegistration?> GetAsync(int tournamentId, int accountId, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(TournamentRegistration entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TournamentRegistration entity, CancellationToken cancellationToken = default);
}