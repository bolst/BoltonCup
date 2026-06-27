namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a team.</summary>
public record TeamDto
{
    /// <summary>Gets the team ID.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the full team name.</summary>
    public required string Name { get; init; }
    /// <summary>Gets the short team name.</summary>
    public required string NameShort { get; init; }
    /// <summary>Gets the team abbreviation.</summary>
    public required string Abbreviation { get; init; }
    /// <summary>Gets the tournament this team participates in.</summary>
    public TournamentBriefDto? Tournament { get; init; }
    /// <summary>Gets the URL of the team logo.</summary>
    public string? LogoUrl { get; init; }
    /// <summary>Gets the URL of the team banner image.</summary>
    public string? BannerUrl { get; init; }
    /// <summary>Gets the primary color hex code.</summary>
    public required string PrimaryColorHex { get; init; }
    /// <summary>Gets the secondary color hex code.</summary>
    public required string SecondaryColorHex { get; init; }
    /// <summary>Gets the tertiary color hex code.</summary>
    public string? TertiaryColorHex { get; init; }
    /// <summary>Gets the URL of the team's goal celebration song, once its track has been downloaded.</summary>
    public string? GoalSongUrl { get; init; }
    /// <summary>Gets the URL of the team's win song, once its track has been downloaded.</summary>
    public string? WinSongUrl { get; init; }
    /// <summary>Gets the account ID of the general manager.</summary>
    public int? GmAccountId { get; init; }
    /// <summary>Gets the first name of the general manager.</summary>
    public string? GmFirstName { get; init; }
    /// <summary>Gets the last name of the general manager.</summary>
    public string? GmLastName { get; init; }
    /// <summary>Gets the URL of the general manager's profile picture.</summary>
    public string? GmProfilePicture  { get; init; }
}

