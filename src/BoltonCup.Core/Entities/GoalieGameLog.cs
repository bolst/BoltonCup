namespace BoltonCup.Core;

public class GoalieGameLog : EntityBase
{
    public required int Id { get; set; }
    public required int PlayerId { get; set; }
    public required int TeamId { get; set; }
    public required int OpponentTeamId { get; set; }
    public required int GameId { get; set; }
    public required int Goals { get; set; }
    public required int Assists { get; set; }
    public required double PenaltyMinutes { get; set; }
    public required int GoalsAgainst { get; set; }
    public required int ShotsAgainst { get; set; }
    public required int Saves { get; set; }
    public required bool Shutout { get; set; }
    public required bool Win { get; set; }
    
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public Team OpponentTeam { get; set; } = null!;
    public Game Game { get; set; } = null!;
}