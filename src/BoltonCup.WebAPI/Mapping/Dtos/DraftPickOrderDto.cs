namespace BoltonCup.WebAPI.Mapping;

public record DraftPickOrderDto
{
    public required int Pick { get; set; }
    public required TeamBriefDto Team { get; set; }
}