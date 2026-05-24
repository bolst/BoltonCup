namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a team.</summary>
public record TeamBriefDto
{
    /// <summary>Gets or sets the team ID.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the full team name.</summary>
    public string? Name { get; set; }
    /// <summary>Gets or sets the short team name.</summary>
    public string? NameShort { get; set; }
    /// <summary>Gets or sets the team abbreviation.</summary>
    public string? Abbreviation { get; set; }
    /// <summary>Gets or sets the URL of the team logo.</summary>
    public string? Logo { get; set; }
    /// <summary>Gets or sets the URL of the team banner image.</summary>
    public string? Banner { get; set; }
    /// <summary>Gets or sets the primary color hex code.</summary>
    public string? PrimaryColorHex { get; set; }
    /// <summary>Gets or sets the secondary color hex code.</summary>
    public string? SecondaryColorHex { get; set; }
    /// <summary>Gets or sets the tertiary color hex code.</summary>
    public string? TertiaryColorHex { get; set; }
}

/// <summary>Brief summary of a team with their goal count for a specific game.</summary>
public record TeamInGameDto : TeamBriefDto
{
    /// <summary>Gets or sets the number of goals scored by the team in the game.</summary>
    public int Goals { get; set; }
}
