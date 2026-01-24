using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;


public record SkaterGameLogDetailDto : IMappable<SkaterGameLog, SkaterGameLogDetailDto>
{
    public PlayerSummary Player { get; init; }
    public TeamSummary Team { get; init; }
    public int OpponentTeamId { get; init; }
    public int GameId { get; init; }
    public int Goals { get; init; }
    public int Assists { get; init; }
    public int Points => Goals + Assists;
    public double PenaltyMinutes { get; init; }

    static Expression<Func<SkaterGameLog, SkaterGameLogDetailDto>> IMappable<SkaterGameLog, SkaterGameLogDetailDto>.Projection =>
        game => new SkaterGameLogDetailDto
        {
            Player = new PlayerSummary(game.Player, game.Player.Account),
            Team = new TeamSummary(game.Team),
            OpponentTeamId = game.OpponentTeamId,
            GameId = game.GameId,
            Goals = game.Goals,
            Assists = game.Assists,
            PenaltyMinutes = game.PenaltyMinutes,
        };
}