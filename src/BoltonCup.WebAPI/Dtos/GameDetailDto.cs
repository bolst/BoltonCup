using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;


public record GameDetailDto : IMappable<Game, GameDetailDto>
{
    public required int Id { get; init; }
    public required DateTime GameTime { get; init; }
    public required TournamentSummary Tournament { get; init; }
    public string? GameType { get; init; }
    public string? Venue  { get; init; }
    public string? Rink { get; init; }
    public GameTeamSummary? HomeTeam { get; init; }
    public GameTeamSummary? AwayTeam { get; init; }

    static Expression<Func<Game, GameDetailDto>> IMappable<Game, GameDetailDto>.Projection =>
        game => new GameDetailDto
        {
            Id = game.Id,
            Tournament = new TournamentSummary(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue, 
            Rink = game.Rink,
            HomeTeam = game.HomeTeam == null ? null : new GameTeamSummary(game.HomeTeam, game.Goals),
            AwayTeam = game.AwayTeam == null ? null : new GameTeamSummary(game.AwayTeam, game.Goals),
        };
}
