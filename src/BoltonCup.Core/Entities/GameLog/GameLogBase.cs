namespace BoltonCup.Core;

public abstract class GameLogBase : EntityBase
{
    public required int Id { get; set; }
    public required int PlayerId { get; set; }
    public required int TeamId { get; set; }
    public required int OpponentTeamId { get; set; }
    public required int GameId { get; set; }
    public required int Goals { get; set; }
    public required int Assists { get; set; }
    public required double PenaltyMinutes { get; set; }
    
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public Team OpponentTeam { get; set; } = null!;
    public Game Game { get; set; } = null!;
}