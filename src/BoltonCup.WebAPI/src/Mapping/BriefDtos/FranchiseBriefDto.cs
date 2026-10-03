namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a franchise and its current brand.</summary>
public record FranchiseBriefDto
{
    /// <summary>Gets the franchise ID.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the full franchise name.</summary>
    public required string Name { get; init; }
    /// <summary>Gets the URL slug that identifies the franchise's page.</summary>
    public required string Slug { get; init; }
    /// <summary>Gets the short franchise name.</summary>
    public required string NameShort { get; init; }
    /// <summary>Gets the franchise abbreviation.</summary>
    public required string Abbreviation { get; init; }
    /// <summary>Gets the URL of the franchise logo.</summary>
    public string? LogoUrl { get; init; }
    /// <summary>Gets the primary color hex code.</summary>
    public required string PrimaryColorHex { get; init; }
    /// <summary>Gets the secondary color hex code.</summary>
    public required string SecondaryColorHex { get; init; }
    /// <summary>Gets the tertiary color hex code.</summary>
    public string? TertiaryColorHex { get; init; }
}