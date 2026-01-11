namespace BoltonCup.Core;

public class GoalieStat
{
    public required int PlayerId { get; set; }
    public required int TeamId { get; set; }
    public required int TournamentId { get; set; }
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public double PenaltyMinutes { get; set; }
    public int GoalsAgainst { get; set; }
    public double GoalsAgainstAverage { get; set; }
    public int ShotsAgainst { get; set; }
    public int Saves { get; set; }
    public double SavePercentage { get; set; }
    public int Shutouts { get; set; }
    public int Wins { get; set; }
    
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public Tournament Tournament { get; set; } = null!;
}