namespace BoltonCup.Core.Commands;

public sealed record UpdateCustomRankingCommand(
    string? Title,
    IReadOnlyList<int>? OrderedPlayerIds
);
