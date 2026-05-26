namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a goalie's statistical line.</summary>
public record GoalieStatDto
{
    /// <summary>Gets the player ID.</summary>
    public required int PlayerId { get; init; }
    /// <summary>Gets the account ID associated with this player.</summary>
    public required int AccountId { get; init; }
    /// <summary>Gets the player's first name.</summary>
    public required string FirstName { get; init; }
    /// <summary>Gets the player's last name.</summary>
    public required string LastName { get; init; }
    /// <summary>Gets the player's position.</summary>
    public required string? Position { get; init; }
    /// <summary>Gets the player's jersey number.</summary>
    public required int? JerseyNumber { get; init; }
    /// <summary>Gets the player's date of birth.</summary>
    public required DateTime Birthday { get; init; }
    /// <summary>Gets the URL of the player's profile picture.</summary>
    public required string? ProfilePicture { get; init; }
    /// <summary>Gets the ID of the player's team.</summary>
    public int? TeamId { get; init; }
    /// <summary>Gets the name of the player's team.</summary>
    public string? TeamName { get; init; }
    /// <summary>Gets the URL of the team logo.</summary>
    public string? TeamLogoUrl { get; init; }
    /// <summary>Gets the team abbreviation.</summary>
    public string? TeamAbbreviation { get; init; }
    /// <summary>Gets the number of games played.</summary>
    public required int GamesPlayed { get; init; }
    /// <summary>Gets the number of goals scored.</summary>
    public required int Goals { get; init; }
    /// <summary>Gets the number of assists.</summary>
    public required int Assists { get; init; }
    /// <summary>Gets the total penalty minutes.</summary>
    public required double PenaltyMinutes { get; init; }
    /// <summary>Gets the total goals allowed.</summary>
    public required int GoalsAgainst { get; init; }
    /// <summary>Gets the goals against average.</summary>
    public required double GoalsAgainstAverage { get; init; }
    /// <summary>Gets the total shots faced.</summary>
    public required int ShotsAgainst { get; init; }
    /// <summary>Gets the total saves made.</summary>
    public required int Saves { get; init; }
    /// <summary>Gets the save percentage.</summary>
    public required double SavePercentage { get; init; }
    /// <summary>Gets the number of shutouts.</summary>
    public required int Shutouts { get; init; }
    /// <summary>Gets the number of wins.</summary>
    public required int Wins { get; init; }
    /// <summary>Gets the player's full name.</summary>
    public string FullName => FirstName + " " + LastName;
}