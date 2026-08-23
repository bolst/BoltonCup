using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Values;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Game ----------

    public GetGamesQuery ToQuery(GetGamesRequest request) => new GetGamesQuery
    {
        TournamentId = request.TournamentId,
        TeamId = request.TeamId,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<GameDto> ToDtoList(IPagedList<Game> games) => games.ProjectTo(game => new GameDto
    {
        Id = game.Id,
        Tournament = ToTournamentBriefDto(game.Tournament),
        GameTime = game.GameTime,
        GameType = game.GameType,
        GameState = game.GameState,
        Venue = game.Venue,
        Rink = game.Rink,
        HomeTeam = ToTeamInGameDto(game, home: true),
        AwayTeam = ToTeamInGameDto(game, home: false),
        HomeTeamPlaceholder = game.HomeTeamPlaceholder,
        AwayTeamPlaceholder = game.AwayTeamPlaceholder,
    });

    public GameSingleDto? ToDto(Game? game, IReadOnlyList<SkaterStat> homeStats, IReadOnlyList<SkaterStat> awayStats) => game is null
            ? null
            : new GameSingleDto
            {
                Id = game.Id,
                Tournament = ToTournamentBriefDto(game.Tournament),
                GameTime = game.GameTime,
                GameType = game.GameType,
                GameState = game.GameState,
                Venue = game.Venue,
                Rink = game.Rink,
                HomeTeam = ToTeamInGameDto(game, home: true),
                AwayTeam = ToTeamInGameDto(game, home: false),
                Goals = game.Goals
                    .Select(ToGoalBriefDto)
                    .OrderBy(g => g.Period)
                    .ThenByDescending(g => g.TimeRemaining)
                    .ToList(),
                Penalties = game.Penalties
                    .Select(ToPenaltyBriefDto)
                    .OrderBy(penalty => penalty.Period)
                    .ThenByDescending(penalty => penalty.TimeRemaining)
                    .ToList(),
                Stars = GetGameStarDtos(game),
                Highlights = game.Highlights
                    .Select(ToGameHighlightDto)
                    .ToList(),
                Officials = game.Referees
                    .OrderBy(r => r.LastName)
                    .ThenBy(r => r.FirstName)
                    .Select(ToRefereeDto)
                    .ToList(),
                PlayersToWatch = homeStats.Count == 0 || awayStats.Count == 0 ? [] :
                [
                    ToGameStatLeaderDto("Points", homeStats.MaxBy(x => x.Points), awayStats.MaxBy(x => x.Points), x => x.Points),
                    ToGameStatLeaderDto("Goals", homeStats.MaxBy(x => x.Goals), awayStats.MaxBy(x => x.Goals), x => x.Goals),
                    ToGameStatLeaderDto("Assists", homeStats.MaxBy(x => x.Assists), awayStats.MaxBy(x => x.Assists), x => x.Assists),
                ],
            };

    static RefereeDto ToRefereeDto(Referee referee) => new RefereeDto
    {
        Id = referee.Id,
        FirstName = referee.FirstName,
        LastName = referee.LastName,
    };

    List<GameStarDto> GetGameStarDtos(Game game) => game.Stars
            .Select(s =>
            {
                List<StatItem> stats;
                if (s.Player.Position == Position.Goalie)
                {
                    var goalsAgainst = game.Goals.Count(t => t.TeamId != s.Player.TeamId);
                    var gaa = (double)goalsAgainst;
                    stats =
                    [
                        new StatItem("GAA", $"{gaa:N2}"),
                    ];

                    if (goalsAgainst == 0)
                    {
                        stats = stats.Append(new StatItem("SO", "1")).ToList();
                    }
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
                    Player: ToPlayerBriefDto(s.Player),
                    Stats: stats
                );
            })
            .OrderBy(gs => gs.StarRank)
            .ToList();

    public GetHighlightsQuery ToQuery(GetHighlightsRequest request) => new GetHighlightsQuery
    {
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<RecentHighlightDto> ToDtoList(IPagedList<GameHighlight> highlights) => highlights.ProjectTo(highlight => new RecentHighlightDto(
        Highlight: ToGameHighlightDto(highlight),
        GameId: highlight.GameId,
        GameTime: highlight.Game.GameTime,
        TournamentName: highlight.Game.Tournament.Name
    ));

    GameHighlightDto ToGameHighlightDto(GameHighlight highlight)
    {
        var highlightUrls = _urlResolver.GetHighlightUrls(highlight.VideoId);
        return new GameHighlightDto(
            VideoUrl: highlightUrls?.VideoUrl ?? string.Empty,
            ThumbnailUrl: highlightUrls?.ThumbnailUrl ?? string.Empty,
            Title: highlight.Title,
            Description: highlight.Description,
            Player: highlight.Player is null ? null : ToPlayerBriefDto(highlight.Player)
        );
    }

    GoalBriefDto ToGoalBriefDto(Goal goal) => new GoalBriefDto
    {
        Id = goal.Id,
        TimeRemaining = goal.PeriodTimeRemaining,
        Period = goal.Period,
        TeamId = goal.TeamId,
        Scorer = ToPlayerBriefDto(goal.Scorer),
        PrimaryAssist = goal.Assist1Player == null ? null : ToPlayerBriefDto(goal.Assist1Player),
        SecondaryAssist = goal.Assist2Player == null ? null : ToPlayerBriefDto(goal.Assist2Player),
        IsEmptyNetGoal = goal.IsEmptyNetGoal,
    };

    PenaltyBriefDto ToPenaltyBriefDto(Penalty penalty) => new PenaltyBriefDto
    {
        Id = penalty.Id,
        TimeRemaining = penalty.PeriodTimeRemaining,
        Period = penalty.Period,
        TeamId = penalty.TeamId,
        Player = ToPlayerBriefDto(penalty.Player),
        Infraction = penalty.InfractionName,
        DurationMins = penalty.DurationMinutes
    };

    public UpdateGameStateCommand ToCommand(int gameId, UpdateGameStateRequest request)
        => new(gameId, request.State, request.IncludePlayerSongs);

    public CreateGoalCommand ToCommand(int gameId, CreateGoalRequest request)
        => new(
            GameId: gameId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            GoalPlayerId: request.GoalPlayerId,
            Assist1PlayerId: request.Assist1PlayerId,
            Assist2PlayerId: request.Assist2PlayerId,
            Notes: request.Notes,
            IsEmptyNetGoal: request.IsEmptyNetGoal
        );

    public UpdateGoalCommand ToCommand(int gameId, int goalId, UpdateGoalRequest request)
        => new(
            GameId: gameId,
            GoalId: goalId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            GoalPlayerId: request.GoalPlayerId,
            Assist1PlayerId: request.Assist1PlayerId,
            Assist2PlayerId: request.Assist2PlayerId,
            Notes: request.Notes,
            IsEmptyNetGoal: request.IsEmptyNetGoal
        );

    public CreatePenaltyCommand ToCommand(int gameId, CreatePenaltyRequest request)
        => new(
            GameId: gameId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            PlayerId: request.PlayerId,
            InfractionName: request.InfractionName,
            DurationMinutes: request.DurationMinutes,
            Notes: request.Notes
        );

    public UpdatePenaltyCommand ToCommand(int gameId, int penaltyId, UpdatePenaltyRequest request)
        => new(
            GameId: gameId,
            PenaltyId: penaltyId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            PlayerId: request.PlayerId,
            InfractionName: request.InfractionName,
            DurationMinutes: request.DurationMinutes,
            Notes: request.Notes
        );

    public SetGameStarsCommand ToCommand(int gameId, SetGameStarsRequest request)
        => new(
            GameId: gameId,
            FirstStarPlayerId: request.FirstStarPlayerId,
            SecondStarPlayerId: request.SecondStarPlayerId,
            ThirdStarPlayerId: request.ThirdStarPlayerId
        );
}
