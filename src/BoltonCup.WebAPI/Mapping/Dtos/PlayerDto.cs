namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player in a tournament.</summary>
public record PlayerDto
{
    /// <summary>Gets the player ID.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the account ID associated with this player.</summary>
    public int? AccountId { get; init; }
    /// <summary>Gets the player's position.</summary>
    public string? Position { get; init; }
    /// <summary>Gets the player's jersey number.</summary>
    public int? JerseyNumber { get; init; }
    /// <summary>Gets the player's first name.</summary>
    public string? FirstName { get; init; }
    /// <summary>Gets the player's last name.</summary>
    public string? LastName { get; init; }
    /// <summary>Gets the player's date of birth.</summary>
    public DateTime? Birthday { get; init; }
    /// <summary>Gets the URL of the player's profile picture.</summary>
    public string? ProfilePicture { get; init; }
    /// <summary>Gets the URL of the player's banner picture.</summary>
    public string? BannerPicture { get; init; }
    /// <summary>Gets the player's preferred beer.</summary>
    public string? PreferredBeer { get; init; }
    /// <summary>Gets the tournament this player is registered in.</summary>
    public required TournamentBriefDto Tournament { get; init; }
    /// <summary>Gets the team this player belongs to, if assigned.</summary>
    public TeamBriefDto? Team { get; init; }

    /// <summary>Gets the player's full name.</summary>
    public string FullName => FirstName + " " + LastName;
    /// <summary>Gets the formatted jersey number label.</summary>
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber.Value}" : string.Empty;
    /// <summary>Gets whether the player is a goalie.</summary>
    public bool IsGoalie => Position == "goalie";
}