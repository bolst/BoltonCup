namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single tournament, including info guide and sponsors.</summary>
public sealed record TournamentSingleDto : TournamentDto
{
    /// <summary>Gets the info guide associated with this tournament, if any.</summary>
    public InfoGuideBriefDto? InfoGuide { get; init; }
    /// <summary>Gets the list of sponsors for this tournament.</summary>
    public List<TournamentSponsorDto> Sponsors { get; init; } = [];
}

/// <summary>DTO representing a tournament sponsor.</summary>
public record TournamentSponsorDto
{
    /// <summary>Gets the sponsor's name.</summary>
    public string? Name { get; init; }
    /// <summary>Gets the URL of the sponsor's logo.</summary>
    public string? LogoUrl { get; init; }
    /// <summary>Gets or sets the sponsor's website URL.</summary>
    public string? WebsiteUrl { get; set; }
}