namespace BoltonCup.Core.Commands;

public sealed record CreateTradeCommand(
    int TournamentId,
    int ProposingTeamId,
    int ReceivingTeamId,
    IReadOnlyList<int> ProposingPlayerIds,
    IReadOnlyList<int> ReceivingPlayerIds,
    string? Note,
    int CreatedByAccountId
);
