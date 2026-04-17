namespace BoltonCup.WebAPI.Mapping;

public record GetGoalieStatsRequest : RequestBase
{
    public int? TournamentId { get; set; }
    public List<int>? TeamIds { get; set; }
    public int? GameId { get; set; }
}