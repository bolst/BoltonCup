namespace BoltonCup.Core;

public class TournamentPlayerGameAvailability : EntityBase
{
    public Guid Id { get; init; }
    public Guid TournamentPlayerInfoId { get; set; }
    public int GameId { get; set; }
    public GameAvailability Availability { get; set; }

    public TournamentPlayerInfo TournamentPlayerInfo { get; set; } = null!;
    public Game Game { get; set; } = null!;
}
