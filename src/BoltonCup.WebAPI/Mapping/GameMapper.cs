using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IGameMapper
{
    GetGamesQuery ToQuery(GetGamesRequest request);
    IPagedList<GameDto> ToDtoList(IPagedList<Game> games);
    GameSingleDto? ToDto(Game? game);
}

public class GameMapper(
    IAssetUrlResolver _urlResolver, 
    IBriefMapper _briefMapper, 
    IGameHighlightMapper _gameHighlightMapper,
    IGameStarMapper _gameStarMapper
) : IGameMapper
{
    public GetGamesQuery ToQuery(GetGamesRequest request)
    {
        return new GetGamesQuery
        {
            TournamentId = request.TournamentId,
            TeamId = request.TeamId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
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
                Stars = game.Stars
                    .Select(_gameStarMapper.ToDto)
                    .OrderBy(gs => gs.StarRank)
                    .ToList(),
                Highlights = game.Highlights
                    .Select(_gameHighlightMapper.ToDto)
                    .ToList(),
            };
    }
}
