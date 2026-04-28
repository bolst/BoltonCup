namespace BoltonCup.WebAPI.Mapping;

public record DraftSingleDto : DraftDto
{
    public required IOrderedEnumerable<DraftPickOrderDto> PickOrder { get; set; }
    public required IOrderedEnumerable<TeamDraftPicks> DraftPicks { get; set; }
}

public sealed record TeamDraftPicks(int TeamId, IOrderedEnumerable<DraftPickDto> Picks);