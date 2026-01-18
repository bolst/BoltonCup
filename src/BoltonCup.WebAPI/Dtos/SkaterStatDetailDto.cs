using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record SkaterStatDetailDto : IMappable<SkaterStat, SkaterStatDetailDto>
{
    public required PlayerSummary Player { get; init; }
    public required TeamSummary Team { get; init; }
    public required TournamentSummary Tournament { get; init; }
    public required int GamesPlayed { get; init; }
    public required int Goals { get; init; }
    public required int Assists { get; init; }
    public required int Points { get; init; }
    public required double PenaltyMinutes { get; init; }
    
    static Expression<Func<SkaterStat, SkaterStatDetailDto>> IMappable<SkaterStat, SkaterStatDetailDto>.Projection =>
        skater => new SkaterStatDetailDto
        {
            Player = new PlayerSummary(skater.Player, skater.Player.Account),
            Team = new TeamSummary(skater.Team),
            Tournament = new TournamentSummary(skater.Tournament),
            GamesPlayed = skater.GamesPlayed,
            Goals = skater.Goals,
            Assists = skater.Assists,
            Points = skater.Points,
            PenaltyMinutes = skater.PenaltyMinutes
        };
}