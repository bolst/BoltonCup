namespace BoltonCup.WebAPI.Models.Base;

public class Team
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string NameShort { get; set; }
    public required string Abbreviation { get; set; }
    public int? TournamentId { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    public string? GoalSongUrl { get; set; }
    public string? PenaltySongUrl { get; set; }
}