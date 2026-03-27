namespace BoltonCup.Core.Commands;

public record CreateTournamentPaymentIntentCommand(
    int AccountId,
    int TournamentId,
    bool IsGoalie
);