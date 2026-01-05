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
    
    public TeamGameSummary? HomeTeam { get; set; }
    public TeamGameSummary? AwayTeam { get; set; }

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
            HomeTeam = game.HomeTeam == null ? null : new TeamGameSummary(game.HomeTeam, game.Goals.Count(g => g.TeamId == game.HomeTeamId)),
            AwayTeam = game.AwayTeam == null ? null : new TeamGameSummary(game.AwayTeam, game.Goals.Count(g => g.TeamId == game.AwayTeamId)),
        };

    public sealed record TeamGameSummary : TeamSummary
    {
        public int Goals { get; set; }

        public TeamGameSummary(Team team, int goals) : base(team)
        {
            Goals = goals;
        }
    }
}
