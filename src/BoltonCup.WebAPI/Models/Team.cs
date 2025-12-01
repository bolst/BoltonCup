namespace BoltonCup.WebAPI.Models;

public class Team
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string NameShort { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    public string? LogoUrl { get; set; }
    public int? TournamentId { get; set; }
    public int? GmAccountId { get; set; }
    public string? BannerImage { get; set; }
    public string? GoalHornUrl { get; set; }
    public string? PenaltySongUrl { get; set; }
}