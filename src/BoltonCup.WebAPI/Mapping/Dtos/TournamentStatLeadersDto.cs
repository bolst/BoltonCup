namespace BoltonCup.WebAPI.Mapping;


/// <summary>DTO representing a leaderboard category with its top players.</summary>
public record PlayerStatLeadersDto
{
    /// <summary>Gets the title of this leaderboard category.</summary>
    public required string Title { get; init; }
    /// <summary>Gets the ranked list of players for this category.</summary>
    public IEnumerable<PlayerStatDto> Leaders { get; init; } = [];
}

/// <summary>DTO representing a player's entry in a stat leaderboard.</summary>
public record PlayerStatDto
{
    /// <summary>Gets the player ID.</summary>
    public int PlayerId { get; init; }
    /// <summary>Gets the account ID associated with this player.</summary>
    public int AccountId { get; init; }
    /// <summary>Gets the player's first name.</summary>
    public string FirstName { get; init; } = string.Empty;
    /// <summary>Gets the player's last name.</summary>
    public string LastName { get; init; } = string.Empty;
    /// <summary>Gets the player's position.</summary>
    public string? Position { get; init; }
    /// <summary>Gets the player's jersey number.</summary>
    public int? JerseyNumber { get; init; }
    /// <summary>Gets the player's date of birth.</summary>
    public DateTime Birthday { get; init; }
    /// <summary>Gets the URL of the player's profile picture.</summary>
    public string? ProfilePicture { get; init; }
    /// <summary>Gets the ID of the player's team.</summary>
    public int TeamId { get; init; }
    /// <summary>Gets the name of the player's team.</summary>
    public string? TeamName { get; init; }
    /// <summary>Gets the URL of the team logo.</summary>
    public string? TeamLogoUrl { get; init; }
    /// <summary>Gets the team abbreviation.</summary>
    public string? TeamAbbreviation { get; init; }
    /// <summary>Gets the raw numeric value of the stat.</summary>
    public required double StatValue { get; init; }
    /// <summary>Gets the formatted display string of the stat.</summary>
    public required string StatString { get; init; }

    /// <summary>Gets the player's full name.</summary>
    public string FullName => FirstName + " " + LastName;
    /// <summary>Gets the formatted jersey number label.</summary>
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber}" : string.Empty;
}