namespace BoltonCup.WebAPI.Mapping;

public record GetGamesRequest : RequestBase
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
}