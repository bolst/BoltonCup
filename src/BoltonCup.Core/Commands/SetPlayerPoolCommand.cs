namespace BoltonCup.Core.Commands;

public sealed record SetPlayerPoolCommand(
    IReadOnlyList<int> OrderedPlayerIds,
    IReadOnlyList<int> ExcludedPlayerIds
);
