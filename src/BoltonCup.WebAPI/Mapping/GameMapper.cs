using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IGameMapper
{
    IPagedList<GameDto> ToDtoList(IPagedList<Game> games);
    GameSingleDto? ToDto(Game? game);
}

public class GameMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IGameMapper
{
    public IPagedList<GameDto> ToDtoList(IPagedList<Game> games)
    {
        return games.ProjectTo(game => new GameDto
        {
            Id = game.Id,
            Tournament = _briefMapper.ToTournamentBriefDto(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue, 
            Rink = game.Rink,
            HomeTeam = _briefMapper.ToTeamInGameDto(game, home: true),
            AwayTeam = _briefMapper.ToTeamInGameDto(game, home: false),
        });
    }    
    
    
    public GameSingleDto? ToDto(Game? game)
    {
        return game is null
            ? null
            : new GameSingleDto 
            { 
                Id = game.Id,
                Tournament = _briefMapper.ToTournamentBriefDto(game.Tournament),
                GameTime = game.GameTime,
                GameType = game.GameType,
                Venue = game.Venue,
                Rink = game.Rink,
                HomeTeam = _briefMapper.ToTeamInGameDto(game, home: true),
                AwayTeam = _briefMapper.ToTeamInGameDto(game, home: false),
                Goals = game.Goals
                    .Select(_briefMapper.ToGoalBriefDto)
                    .OrderBy(g => g.Period)
                    .ThenByDescending(g => g.TimeRemaining)
                    .ToList(),
                Penalties = game.Penalties
                    .Select(_briefMapper.ToPenaltyBriefDto)
                    .OrderBy(penalty => penalty.Period)
                    .ThenByDescending(penalty => penalty.TimeRemaining)
                    .ToList(),
            };
    }
}
