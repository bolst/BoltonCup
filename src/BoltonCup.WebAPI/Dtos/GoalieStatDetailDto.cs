using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record GoalieStatDetailDto : IMappable<GoalieStat, GoalieStatDetailDto>
{
    public required PlayerSummary Player { get; init; }
    public required TeamSummary Team { get; init; }
    public required TournamentSummary Tournament { get; init; }
    public required int GamesPlayed { get; init; }
    public required int Goals { get; init; }
    public required int Assists { get; init; }
    public required double PenaltyMinutes { get; init; }
    public required int GoalsAgainst { get; init; }
    public required double GoalsAgainstAverage { get; init; }
    public required int ShotsAgainst { get; init; }
    public required int Saves { get; init; }
    public required double SavePercentage { get; init; }
    public required int Shutouts { get; init; }
    public required int Wins { get; init; }
    
    static Expression<Func<GoalieStat, GoalieStatDetailDto>> IMappable<GoalieStat, GoalieStatDetailDto>.Projection =>
        goalie => new GoalieStatDetailDto
        {
            Player = new PlayerSummary(goalie.Player, goalie.Player.Account),
            Team = new TeamSummary(goalie.Team),
            Tournament = new TournamentSummary(goalie.Tournament),
            GamesPlayed = goalie.GamesPlayed,
            Goals = goalie.Goals,
            Assists = goalie.Assists,
            PenaltyMinutes = goalie.PenaltyMinutes,
            GoalsAgainst = goalie.GoalsAgainst,
            GoalsAgainstAverage = goalie.GoalsAgainstAverage,
            ShotsAgainst = goalie.ShotsAgainst,
            Saves = goalie.Saves,
            SavePercentage = goalie.SavePercentage,
            Shutouts = goalie.Shutouts,
            Wins = goalie.Wins
        };
}