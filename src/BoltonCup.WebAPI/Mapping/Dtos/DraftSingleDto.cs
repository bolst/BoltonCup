namespace BoltonCup.WebAPI.Mapping;

public record DraftSingleDto : DraftDto
{
    public required IOrderedEnumerable<DraftPickOrderDto> PickOrder { get; set; }
}