namespace BoltonCup.WebAPI.Data.Entities;

public class Team : EntityBase
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
    
    public Tournament Tournament { get; set; }
    public ICollection<Player> Players { get; set; }
    public ICollection<Game> HomeGames { get; set; }
    public ICollection<Game> AwayGames { get; set; }
}