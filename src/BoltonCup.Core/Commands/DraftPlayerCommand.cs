namespace BoltonCup.Core.Commands;

public sealed record DraftPlayerCommand(
    int DraftId,
    int PlayerId,
    int TeamId,
    int OverallPick
);