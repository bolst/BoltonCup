namespace BoltonCup.WebAPI.Mapping;

public record DraftUpdateEventDto(
    DraftSingleDto Draft,
    DraftPickSingleDto? NextPick
);