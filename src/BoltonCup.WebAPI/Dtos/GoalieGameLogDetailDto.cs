using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;


public record GoalieGameLogDetailDto : IMappable<GoalieGameLog, GoalieGameLogDetailDto>
{
    public PlayerSummary Player { get; init; }
    public TeamSummary Team { get; init; }
    public int OpponentTeamId { get; init; }
    public int GameId { get; init; }
    public int Goals { get; init; }
    public int Assists { get; init; }
    public int Points => Goals + Assists;
    public double PenaltyMinutes { get; init; }
    public required int GoalsAgainst { get; init; }
    public required double GoalsAgainstAverage { get; init; }
    public required int ShotsAgainst { get; init; }
    public required int Saves { get; init; }
    public required double SavePercentage { get; init; }
    public required bool Shutout { get; init; }
    public required bool Win { get; init; }

    static Expression<Func<GoalieGameLog, GoalieGameLogDetailDto>> IMappable<GoalieGameLog, GoalieGameLogDetailDto>.Projection =>
        game => new GoalieGameLogDetailDto
        {
            Player = new PlayerSummary(game.Player, game.Player.Account),
            Team = new TeamSummary(game.Team),
            OpponentTeamId = game.OpponentTeamId,
            GameId = game.GameId,
            Goals = game.Goals,
            Assists = game.Assists,
            PenaltyMinutes = game.PenaltyMinutes,
            GoalsAgainst = game.GoalsAgainst,
            GoalsAgainstAverage = game.GoalsAgainst / 60.0,
            ShotsAgainst = game.ShotsAgainst,
            Saves = game.Saves,
            SavePercentage = game.Saves == 0 ? 0 : (double)game.ShotsAgainst / game.Saves,
            Shutout = game.Shutout,
            Win = game.Win,
        };
}