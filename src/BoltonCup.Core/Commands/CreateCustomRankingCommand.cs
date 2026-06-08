namespace BoltonCup.Core.Commands;

public sealed record CreateCustomRankingCommand(
    int TournamentId,
    string Title,
    int OwnerAccountId
);
