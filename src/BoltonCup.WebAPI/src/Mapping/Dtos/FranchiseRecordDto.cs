namespace BoltonCup.WebAPI.Mapping;

/// <summary>
/// Win/loss totals over completed games. Losses include overtime and shootout losses; a forfeit (completed 0–0)
/// counts as a tie.
/// </summary>
public record FranchiseRecordDto
{
    /// <summary>Gets the number of completed games played.</summary>
    public required int GamesPlayed { get; init; }
    /// <summary>Gets the number of wins.</summary>
    public required int Wins { get; init; }
    /// <summary>Gets the number of losses, including overtime and shootout losses.</summary>
    public required int Losses { get; init; }
    /// <summary>Gets the number of ties.</summary>
    public required int Ties { get; init; }
    /// <summary>Gets the number of goals scored.</summary>
    public required int GoalsFor { get; init; }
    /// <summary>Gets the number of goals conceded.</summary>
    public required int GoalsAgainst { get; init; }
}