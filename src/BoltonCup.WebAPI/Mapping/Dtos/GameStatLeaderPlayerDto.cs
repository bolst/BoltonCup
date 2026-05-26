namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing the leader of a single stat category for one team in a game.</summary>
public record GameStatLeaderPlayerDto
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
    /// <summary>Gets the URL of the player's profile picture.</summary>
    public string? ProfilePicture { get; init; }
    /// <summary>Gets the raw numeric value of the stat.</summary>
    public required double StatValue { get; init; }
    /// <summary>Gets the formatted display string of the stat.</summary>
    public required string StatString { get; init; }

    /// <summary>Gets the player's full name.</summary>
    public string FullName => FirstName + " " + LastName;
    /// <summary>Gets the formatted jersey number label.</summary>
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber}" : string.Empty;
}
