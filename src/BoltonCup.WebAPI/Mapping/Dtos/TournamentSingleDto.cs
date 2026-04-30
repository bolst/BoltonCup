namespace BoltonCup.WebAPI.Mapping;

public sealed record TournamentSingleDto : TournamentDto
{
    public InfoGuideBriefDto? InfoGuide { get; init; }
    public List<TournamentSponsorDto> Sponsors { get; init; } = [];
}


public record TournamentSponsorDto
{
    public string? Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? WebsiteUrl { get; set; }
}