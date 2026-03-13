using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface IGameMapper
{
    IPagedList<GameDto> ToDtoList(IPagedList<Game> games);
    GameSingleDto? ToDto(Game? game);
}

public class GameMapper(IAssetUrlResolver _urlResolver) : IGameMapper
{
    public IPagedList<GameDto> ToDtoList(IPagedList<Game> games)
    {
        return games.ProjectTo(game => new GameDto
        {
            Id = game.Id,
            Tournament = new TournamentBriefDto(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue, 
            Rink = game.Rink,
            HomeTeam = game.HomeTeam == null ? null : new TeamInGameDto(game.HomeTeam, game.Goals),
            AwayTeam = game.AwayTeam == null ? null : new TeamInGameDto(game.AwayTeam, game.Goals),
        });
    }    
    
    
    public GameSingleDto? ToDto(Game? game)
    {
        return game is null
            ? null
            : new GameSingleDto 
            { 
                Id = game.Id,
                Tournament = new TournamentBriefDto(game.Tournament),
                GameTime = game.GameTime,
                GameType = game.GameType,
                Venue = game.Venue,
                Rink = game.Rink,
                HomeTeam = game.HomeTeam == null ? null : new TeamInGameDto(game.HomeTeam, game.Goals),
                AwayTeam = game.AwayTeam == null ? null : new TeamInGameDto(game.AwayTeam, game.Goals),
                Goals = game.Goals
                    .Select(goal => new GoalBriefDto
                    {
                        TimeRemaining = goal.PeriodTimeRemaining,
                        Period = goal.Period,
                        PeriodLabel = goal.PeriodLabel,
                        TeamId = goal.TeamId,
                        Scorer = new PlayerBriefDto(goal.Scorer, goal.Scorer.Account),
                        PrimaryAssist = goal.Assist1Player == null ? null : new PlayerBriefDto(goal.Assist1Player, goal.Assist1Player.Account),
                        SecondaryAssist = goal.Assist2Player == null ? null : new PlayerBriefDto(goal.Assist2Player, goal.Assist2Player.Account),
                    })
                    .OrderBy(g => g.Period)
                    .ThenByDescending(g => g.TimeRemaining)
                    .ToList(),
                Penalties = game.Penalties
                    .Select(penalty => new PenaltyBriefDto
                    {
                        TimeRemaining = penalty.PeriodTimeRemaining,
                        Period = penalty.Period,
                        PeriodLabel = penalty.PeriodLabel,
                        TeamId = penalty.TeamId,
                        Player = new PlayerBriefDto(penalty.Player, penalty.Player.Account),
                        Infraction = penalty.InfractionName,
                        DurationMins = penalty.DurationMinutes
                    })
                    .OrderBy(penalty => penalty.Period)
                    .ThenByDescending(penalty => penalty.TimeRemaining)
                    .ToList(),
            };
    }
}
