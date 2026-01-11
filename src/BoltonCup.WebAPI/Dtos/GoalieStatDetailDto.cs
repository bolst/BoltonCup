using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record GoalieStatDetailDto : IMappable<GoalieStat, GoalieStatDetailDto>
{
    public required PlayerSummary Player { get; init; }
    public required TeamSummary Team { get; init; }
    public required int TournamentId { get; init; }
    public required string TournamentName { get; init; }
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
        skater => new GoalieStatDetailDto
        {
            Player = new PlayerSummary(skater.Player, skater.Player.Account),
            Team = new TeamSummary(skater.Team),
            TournamentId = skater.TournamentId,
            TournamentName = skater.Tournament.Name,
            GamesPlayed = skater.GamesPlayed,
            Goals = skater.Goals,
            Assists = skater.Assists,
            PenaltyMinutes = skater.PenaltyMinutes,
            GoalsAgainst = skater.GoalsAgainst,
            GoalsAgainstAverage = skater.GoalsAgainstAverage,
            ShotsAgainst = skater.ShotsAgainst,
            Saves = skater.Saves,
            SavePercentage = skater.SavePercentage,
            Shutouts = skater.Shutouts,
            Wins = skater.Wins
        };
}