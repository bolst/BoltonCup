using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;

namespace BoltonCup.WebAPI.Dtos;


public record GameDetailDto : IMappable<Game, GameDetailDto>
{
    public required int Id { get; set; }
    public required int TournamentId { get; set; }
    public required string TournamentName { get; set; }
    public required DateTime GameTime { get; set; }
    public string? GameType { get; set; }
    public string? Venue  { get; set; }
    public string? Rink { get; set; }
    
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public string? HomeTeamName { get; set; }
    public string? AwayTeamName { get; set; }
    public string? HomeTeamNameShort { get; set; }
    public string? AwayTeamNameShort { get; set; }
    public string? HomeTeamAbbreviation { get; set; }
    public string? AwayTeamAbbreviation { get; set; }
    public string? HomeTeamLogoUrl { get; set; }
    public string? AwayTeamLogoUrl { get; set; }
    public string? HomeTeamBannerUrl { get; set; }
    public string? AwayTeamBannerUrl { get; set; }
    public required int HomeGoals { get; set; }
    public required int AwayGoals { get; set; }

    static Expression<Func<Game, GameDetailDto>> IMappable<Game, GameDetailDto>.Projection =>
        game => new GameDetailDto
        {
            Id = game.Id,
            TournamentId = game.TournamentId,
            TournamentName = game.Tournament.Name,
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue, 
            Rink = game.Rink,
            HomeTeamId = game.HomeTeamId,
            AwayTeamId = game.AwayTeamId,
            HomeTeamName = game.HomeTeam!.Name,
            AwayTeamName = game.AwayTeam!.Name,
            HomeTeamNameShort = game.HomeTeam.NameShort,
            AwayTeamNameShort = game.AwayTeam.NameShort,
            HomeTeamAbbreviation = game.HomeTeam.Abbreviation,
            AwayTeamAbbreviation = game.AwayTeam.Abbreviation,
            HomeTeamLogoUrl = game.HomeTeam.LogoUrl,
            AwayTeamLogoUrl = game.AwayTeam.LogoUrl,
            HomeTeamBannerUrl = game.HomeTeam.BannerUrl,
            AwayTeamBannerUrl = game.AwayTeam.BannerUrl,
            HomeGoals = game.Goals.Count(g => g.TeamId == game.HomeTeamId),
            AwayGoals = game.Goals.Count(g => g.TeamId == game.AwayTeamId),
        };
}
