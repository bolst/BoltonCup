namespace BoltonCup.WebAPI.Mapping;

/// <summary>A single team's row in a standings table.</summary>
public record StandingRowDto
{
    /// <summary>Gets the team's position in the standings (1-based).</summary>
    public int Rank { get; init; }
    /// <summary>Gets the team ID.</summary>
    public int TeamId { get; init; }
    /// <summary>Gets the full team name.</summary>
    public string? Name { get; init; }
    /// <summary>Gets the short team name.</summary>
    public string? NameShort { get; init; }
    /// <summary>Gets the team abbreviation.</summary>
    public string? Abbreviation { get; init; }
    /// <summary>Gets the URL of the team logo.</summary>
    public string? Logo { get; init; }
    /// <summary>Gets the primary color hex code.</summary>
    public string? PrimaryColorHex { get; init; }
    /// <summary>Gets the secondary color hex code.</summary>
    public string? SecondaryColorHex { get; init; }
    /// <summary>Gets the tertiary color hex code.</summary>
    public string? TertiaryColorHex { get; init; }
    /// <summary>Gets the number of games played.</summary>
    public int GamesPlayed { get; init; }
    /// <summary>Gets the total number of wins (including OT/SO).</summary>
    public int Wins { get; init; }
    /// <summary>Gets the number of regulation wins.</summary>
    public int RegulationWins { get; init; }
    /// <summary>Gets the number of regulation losses.</summary>
    public int Losses { get; init; }
    /// <summary>Gets the number of ties.</summary>
    public int Ties { get; init; }
    /// <summary>Gets the number of OT/SO losses.</summary>
    public int OtSoLosses { get; init; }
    /// <summary>Gets the goals scored.</summary>
    public int GoalsFor { get; init; }
    /// <summary>Gets the goals conceded.</summary>
    public int GoalsAgainst { get; init; }
    /// <summary>Gets the goal differential (goals for minus goals against).</summary>
    public int GoalDifferential { get; init; }
    /// <summary>Gets the total points.</summary>
    public int Points { get; init; }
}
