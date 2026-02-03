using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record SkaterStatDetailDto : IMappable<SkaterStat, SkaterStatDetailDto>
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
    public required int Points { get; init; }
    public required double PenaltyMinutes { get; init; }
    
    public string FullName => FirstName + " " + LastName;
    
    static Expression<Func<SkaterStat, SkaterStatDetailDto>> IMappable<SkaterStat, SkaterStatDetailDto>.Projection =>
        skater => new SkaterStatDetailDto
        {
            PlayerId = skater.PlayerId,
            AccountId = skater.AccountId,
            FirstName = skater.FirstName,
            LastName = skater.LastName,
            Position = skater.Position,
            JerseyNumber = skater.JerseyNumber,
            Birthday = skater.Birthday,
            ProfilePicture = skater.ProfilePicture,
            TeamId = skater.TeamId,
            TeamName = skater.TeamName,
            TeamLogoUrl = skater.TeamLogoUrl,
            GamesPlayed = skater.GamesPlayed,
            Goals = skater.Goals,
            Assists = skater.Assists,
            Points = skater.Points,
            PenaltyMinutes = skater.PenaltyMinutes
        };
}