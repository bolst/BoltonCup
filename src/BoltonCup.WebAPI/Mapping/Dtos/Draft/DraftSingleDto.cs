namespace BoltonCup.WebAPI.Mapping;

public record DraftSingleDto : DraftDto
{
    public required IOrderedEnumerable<DraftPickOrderDto> PickOrder { get; set; }
    public required IOrderedEnumerable<RoundDraftPicks> DraftPicksByRound { get; set; }
    public bool CanEditDraft { get; set; }
}

public sealed record RoundDraftPicks(int Round, IOrderedEnumerable<DraftPickDto> Picks);