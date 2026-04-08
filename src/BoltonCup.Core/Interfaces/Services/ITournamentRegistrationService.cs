namespace BoltonCup.Core;

public interface ITournamentRegistrationService
{
    Task<TournamentRegistration?> GetAsync(int tournamentId, int accountId, CancellationToken cancellationToken = default);
    Task UpsertAsync(UpsertTournamentRegistrationCommand command, CancellationToken cancellationToken = default);
    Task CompleteRegistrationAsync(int accountId, int tournamentId, string paymentId, CancellationToken cancellationToken = default);
}

public record UpsertTournamentRegistrationCommand(int TournamentId, int AccountId, int CurrentStep, bool IsComplete, string? Payload);