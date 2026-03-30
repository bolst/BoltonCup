namespace BoltonCup.WebAPI.Mapping;

public record GetSkaterStatsRequest : RequestBase
{
    public int? TournamentId { get; set; }
    public string? Position { get; set; }
    public List<int>? TeamIds { get; set; }
}