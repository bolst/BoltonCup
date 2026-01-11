namespace BoltonCup.Core;

public class SkaterStat
{
    public required int PlayerId { get; set; }
    public required int TeamId { get; set; }
    public required int TournamentId { get; set; }
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Points { get; set; }
    public double PenaltyMinutes { get; set; }
    
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public Tournament Tournament { get; set; } = null!;
}