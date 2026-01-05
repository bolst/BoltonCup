using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

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
    
    public GameTeamSummary? HomeTeam { get; set; }
    public GameTeamSummary? AwayTeam { get; set; }

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
            HomeTeam = game.HomeTeam == null ? null : new GameTeamSummary(game.HomeTeam, game.Goals),
            AwayTeam = game.AwayTeam == null ? null : new GameTeamSummary(game.AwayTeam, game.Goals),
        };
}
