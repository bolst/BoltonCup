using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record DraftPickMadeEventDto(
    int DraftId,
    DraftPickSingleDto CompletedPick,
    DraftPickSingleDto? NextPick,
    DraftStatus NewDraftStatus
);