using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record GameSingleDetailDto : GameDetailDto, IMappable<Game, GameSingleDetailDto>
{
    static Expression<Func<Game, GameSingleDetailDto>> IMappable<Game, GameSingleDetailDto>.Projection =>
        game => new GameSingleDetailDto
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