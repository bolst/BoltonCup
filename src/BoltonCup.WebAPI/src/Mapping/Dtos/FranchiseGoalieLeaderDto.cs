namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a goalie's totals across every season played for a franchise.</summary>
public record FranchiseGoalieLeaderDto
{
    /// <summary>Gets the account ID of the goalie.</summary>
    public required int AccountId { get; init; }
    /// <summary>Gets the latest player ID of the goalie.</summary>
    public required int PlayerId { get; init; }
    /// <summary>Gets the first name of the goalie.</summary>
    public required string FirstName { get; init; }
    /// <summary>Gets the last name of the goalie.</summary>
    public required string LastName { get; init; }
    /// <summary>Gets the URL of the goalie's profile picture.</summary>
    public string? ProfilePictureUrl { get; init; }
    /// <summary>Gets the number of seasons the goalie played for the franchise.</summary>
    public required int Seasons { get; init; }
    /// <summary>Gets the number of games played.</summary>
    public required int GamesPlayed { get; init; }
    /// <summary>Gets the number of wins.</summary>
    public required int Wins { get; init; }
    /// <summary>Gets the number of shutouts.</summary>
    public required int Shutouts { get; init; }
    /// <summary>Gets the number of saves.</summary>
    public required int Saves { get; init; }
    /// <summary>Gets the number of shots against.</summary>
    public required int ShotsAgainst { get; init; }
    /// <summary>Gets the save percentage (saves divided by shots against); 0 when no shots were faced.</summary>
    public required double SavePercentage { get; init; }
    /// <summary>Gets the goals-against average: goals against per game played, unweighted by minutes.</summary>
    public required double GoalsAgainstAverage { get; init; }
}