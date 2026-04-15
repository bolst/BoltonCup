namespace BoltonCup.WebAPI.Mapping;

public record BracketChallengeDto
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Link { get; init; }
    public decimal? Fee { get; init; }
    public bool IsOpen { get; init; }
    public string? Logo { get; init; }
    public DateTime? CloseDate { get; init; }
}
