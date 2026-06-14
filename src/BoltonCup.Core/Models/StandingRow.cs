namespace BoltonCup.Core;

/// <summary>A computed standings row for a single team within a stage. Not persisted.</summary>
public sealed record StandingRow
{
    public required Team Team { get; init; }
    public int TeamId => Team.Id;

    public int Rank { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int RegulationWins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int OtSoLosses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Points { get; set; }

    public int GoalDifferential => GoalsFor - GoalsAgainst;
}
