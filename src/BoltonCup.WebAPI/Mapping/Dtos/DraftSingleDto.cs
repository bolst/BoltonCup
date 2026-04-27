namespace BoltonCup.WebAPI.Mapping;

public record DraftSingleDto : DraftDto
{
    public required IOrderedEnumerable<DraftPickOrderDto> PickOrder { get; set; }
    public required IOrderedEnumerable<DraftPickDto> DraftPicks { get; set; }
}