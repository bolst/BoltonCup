namespace BoltonCup.WebAPI.Models.Base;

public class Player
{
    public required Guid Id { get; set; }
    public required int TournamentId { get; set; }
    public int? TeamId { get; set; }
    public string? Position { get; set; }
    public string? PreferredBeer { get; set; }
    public int? JerseyNumber { get; set; }
}