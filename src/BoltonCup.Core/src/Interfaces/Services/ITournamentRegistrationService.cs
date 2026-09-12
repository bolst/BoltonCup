namespace BoltonCup.Core;

public interface ITournamentRegistrationService
{
    Task<TournamentRegistration?> GetAsync(int tournamentId, int accountId, CancellationToken cancellationToken = default);
    Task UpsertAsync(UpsertTournamentRegistrationCommand command, CancellationToken cancellationToken = default);
    Task CompleteRegistrationAsync(int accountId, int tournamentId, string paymentId, CancellationToken cancellationToken = default);

    /// <summary>Validates a tournament registration is eligible for payment (tournament exists,
    /// registration is open, a fee is configured for the position, the account exists and isn't
    /// already registered) and returns the data needed to create a payment intent.</summary>
    Task<TournamentPaymentPreparation> PrepareTournamentPaymentAsync(int tournamentId, int accountId, bool isGoalie, CancellationToken cancellationToken = default);
}

public record UpsertTournamentRegistrationCommand(int TournamentId, int AccountId, int CurrentStep, bool IsComplete, string? Payload);

public record TournamentPaymentPreparation(int TournamentId, int AccountId, string? AccountEmail, decimal RegistrationFee);