namespace BoltonCup.WebAPI.Mapping;

public record GetTeamsRequest : RequestBase
{
    public int? TournamentId { get; set; }
}