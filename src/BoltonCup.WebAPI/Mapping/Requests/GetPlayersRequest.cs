namespace BoltonCup.WebAPI.Mapping;

public record GetPlayersRequest : RequestBase
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
}