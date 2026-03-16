namespace BoltonCup.WebAPI.Mapping.Core;

public record TeamDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string NameShort { get; init; }
    public required string Abbreviation { get; init; }
    public TournamentBriefDto? Tournament { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public required string PrimaryColorHex { get; init; }
    public required string SecondaryColorHex { get; init; }
    public string? TertiaryColorHex { get; init; }
    public string? GoalSongUrl { get; init; }
    public string? PenaltySongUrl { get; init; }
    
    public int? GmAccountId { get; init; }
    public string? GmFirstName { get; init; }
    public string? GmLastName { get; init; }
    public string? GmProfilePicture  { get; init; }
}

