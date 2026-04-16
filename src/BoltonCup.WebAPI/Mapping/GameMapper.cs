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
    IGameHighlightMapper _gameHighlightMapper
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
                Stars = GetGameStarDtos(game),
                Highlights = game.Highlights
                    .Select(_gameHighlightMapper.ToDto)
                    .ToList(),
            };
    }


    private List<GameStarDto> GetGameStarDtos(Game game)
    {
        return game.Stars
            .Select(s =>
            {
                List<StatItem> stats;
                if (s.Player.Position == Core.Values.Position.Goalie)
                {
                    var goalsAgainst = game.Goals.Count(t => t.TeamId != s.Player.TeamId);
                    var gaa = (double)goalsAgainst;
                    stats =
                    [
                        new StatItem("GAA", $"{gaa:N2}"), 
                    ];

                    if (goalsAgainst == 0)
                        stats = stats.Append(new StatItem("SO", "1")).ToList();
                }
                else
                {
                    var goals = game.Goals.Count(g => g.GoalPlayerId == s.Player.Id);
                    var assists = game.Goals.Count(g => g.Assist1PlayerId == s.Player.Id || g.Assist2PlayerId == s.Player.Id);
                    var points = goals + assists;
                    stats =
                    [
                        new StatItem("G", goals.ToString()), 
                        new StatItem("A", assists.ToString()),
                        new StatItem("P", points.ToString())
                    ];
                }
                
                return new GameStarDto(
                    StarRank: s.StarRank,
                    Player: _briefMapper.ToPlayerBriefDto(s.Player),
                    Stats: stats
                );
            })
            .OrderBy(gs => gs.StarRank)
            .ToList();
    }
}
