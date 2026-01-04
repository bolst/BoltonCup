namespace BoltonCup.Infrastructure.Data.Entities;

public class Team : EntityBase
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string NameShort { get; set; }
    public required string Abbreviation { get; set; }
    public int? TournamentId { get; set; }
    public int? GmAccountId { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    public string? GoalSongUrl { get; set; }
    public string? PenaltySongUrl { get; set; }
    
    public Tournament Tournament { get; set; }
    public Account? GeneralManager { get; set; }
    public ICollection<Player> Players { get; set; }
    public ICollection<Game> HomeGames { get; set; }
    public ICollection<Game> AwayGames { get; set; }
    public ICollection<Goal> Goals { get; set; }
    public ICollection<Penalty> Penalties { get; set; }
}