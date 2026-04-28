namespace BoltonCup.WebAPI.Mapping;

public record DraftPickMadeEventDto(
    int DraftId,
    DraftPickBriefDto CompletedPick,
    PlayerBriefDto DraftedPlayer,
    DraftPickSingleDto? NextPick
);