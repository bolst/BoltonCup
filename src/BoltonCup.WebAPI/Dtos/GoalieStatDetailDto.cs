using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record GoalieStatDetailDto : IMappable<GoalieStat, GoalieStatDetailDto>
{
    public required int PlayerId { get; init; }
    public required int AccountId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string? Position { get; init; }
    public required int? JerseyNumber { get; init; }
    public required DateTime Birthday { get; init; }
    public required string? ProfilePicture { get; init; }
    public int? TeamId { get; init; }
    public string? TeamName { get; init; }
    public string? TeamLogoUrl { get; init; }
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
    
    public string FullName => FirstName + " " + LastName;

    
    static Expression<Func<GoalieStat, GoalieStatDetailDto>> IMappable<GoalieStat, GoalieStatDetailDto>.Projection =>
        goalie => new GoalieStatDetailDto
        {
            PlayerId = goalie.PlayerId,
            AccountId = goalie.AccountId,
            FirstName = goalie.FirstName,
            LastName = goalie.LastName,
            Position = goalie.Position,
            JerseyNumber = goalie.JerseyNumber,
            Birthday = goalie.Birthday,
            ProfilePicture = goalie.ProfilePicture,
            TeamId = goalie.TeamId,
            TeamName = goalie.TeamName,
            TeamLogoUrl = goalie.TeamLogoUrl,
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