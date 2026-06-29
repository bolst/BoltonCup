namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a player.</summary>
public record PlayerBriefDto
{
    /// <summary>Gets or sets the player ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the account ID associated with this player.</summary>
    public int AccountId { get; set; }
    /// <summary>Gets or sets the player's position.</summary>
    public string? Position { get; set; }
    /// <summary>Gets or sets the player's jersey number.</summary>
    public int? JerseyNumber { get; set; }
    /// <summary>Gets or sets the player's first name.</summary>
    public required string FirstName { get; set; }
    /// <summary>Gets or sets the player's last name.</summary>
    public required string LastName { get; set; }
    /// <summary>Gets or sets the player's date of birth.</summary>
    public DateTime Birthday { get; set; }
    /// <summary>Gets or sets the URL of the player's profile picture.</summary>
    public string? ProfilePicture { get; set; }
    /// <summary>Gets or sets the captaincy designation character (C or A).</summary>
    public char? CaptaincyTag { get; set; }
    /// <summary>Gets or sets whether the player can play both forward and defense.</summary>
    public bool CanPlayEitherPosition { get; set; }
    /// <summary>Gets whether the player is a goalie.</summary>
    public bool IsGoalie => Position == BoltonCup.Core.Values.Position.Goalie;
    /// <summary>Gets the player's full name.</summary>
    public string FullName => FirstName + " " + LastName;
    /// <summary>Gets the formatted jersey number label.</summary>
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber.Value}" : string.Empty;
}