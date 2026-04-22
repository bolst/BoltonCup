namespace BoltonCup.Core.Commands;

public sealed record UpdateDraftOrderingCommand(
    int DraftId,
    List<DraftOrderCommandEntry> Ordering
);

public sealed record DraftOrderCommandEntry(int TeamId, int Pick);