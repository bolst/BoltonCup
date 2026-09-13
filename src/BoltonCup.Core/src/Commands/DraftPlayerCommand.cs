namespace BoltonCup.Core.Commands;

public sealed record DraftPlayerCommand(
    int DraftId,
    int PlayerId,
    int TeamId,
    int OverallPick,
    bool IsAutoPick = false
);

public sealed record ReplaceDraftPickCommand(
    int DraftId,
    int OverallPick,
    int NewPlayerId
);