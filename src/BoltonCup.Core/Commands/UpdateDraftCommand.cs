namespace BoltonCup.Core.Commands;

public sealed record UpdateDraftCommand(
    int DraftId,
    DraftType DraftType
);