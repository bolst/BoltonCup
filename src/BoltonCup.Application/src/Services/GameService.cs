using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using BoltonCup.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Application.Services;

class GameService(
    BoltonCupDbContext _dbContext,
    IGlobalMusicQueue _queue,
    IMusicLibraryService _music,
    IStatisticsRefreshService _stats,
    TimeProvider _timeProvider,
    IMemoryCache _cache,
    ILogger<GameService> _logger) : IGameService
{
    const string GameDaysCacheKey = "games:scores:days";
    static readonly TimeSpan GameDaysCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IPagedList<Game>> GetAllAsync(GetGamesQuery query, CancellationToken cancellationToken = default) => await _dbContext.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.HomeTeamId == query.TeamId || p.AwayTeamId == query.TeamId, query.TeamId.HasValue)
            .ApplySorting(
                query,
                x => x
                    .OrderByDescending(p => p.Tournament.StartDate)
                    .ThenBy(p => p.GameTime)
                    .ThenBy(p => p.GameState == GameState.InProgress ? 0 : (p.GameState == GameState.Pending ? 1 : 2))
            )
            .ToPagedListAsync(query, cancellationToken: cancellationToken);

    public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await _dbContext.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
                .ThenInclude(t => t!.GoalSongTrack)
            .Include(p => p.HomeTeam)
                .ThenInclude(t => t!.PenaltySongTrack)
            .Include(p => p.HomeTeam)
                .ThenInclude(t => t!.WinSongTrack)
            .Include(p => p.AwayTeam)
                .ThenInclude(t => t!.GoalSongTrack)
            .Include(p => p.AwayTeam)
                .ThenInclude(t => t!.PenaltySongTrack)
            .Include(p => p.AwayTeam)
                .ThenInclude(t => t!.WinSongTrack)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Scorer)
                .ThenInclude(p => p.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist1Player)
                .ThenInclude(p => p!.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist2Player)
                .ThenInclude(p => p!.Account)
            .Include(p => p.Penalties)
                .ThenInclude(x => x.Player)
                .ThenInclude(x => x.Account)
            .Include(p => p.Stars)
                .ThenInclude(s => s.Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Referees)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);

    public async Task<GameScores> GetScoresAsync(GetScoresQuery query, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var zone = EasternTime.GetTimeZone();
        var days = await GetGameDaysAsync(zone, cancellationToken);

        var gameDates = days
            .Select(d => d.Date)
            .Distinct()
            .ToList();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        var date = query.Date ?? PickDefaultDate(gameDates, today);
        var previousDate = gameDates
            .Where(d => d < date)
            .Select(d => (DateOnly?)d)
            .LastOrDefault();
        var nextDate = FirstAfter(gameDates, date);

        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), zone);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue), zone);

        var games = await _dbContext.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Scorer)
                .ThenInclude(p => p.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist1Player)
                .ThenInclude(p => p!.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist2Player)
                .ThenInclude(p => p!.Account)
            .Where(p => p.GameTime >= dayStartUtc && p.GameTime < dayEndUtc)
            .OrderBy(p => p.GameTime)
            .ToListAsync(cancellationToken);

        return new GameScores(
            Date: date,
            PreviousDate: previousDate,
            NextDate: nextDate,
            Days: days,
            Games: games);
    }

    // Every scores request (and every live poll) needs the full day index, so cache it briefly.
    // A game added or rescheduled can take up to GameDaysCacheDuration to show in the date strip.
    async Task<IReadOnlyList<GameDay>> GetGameDaysAsync(TimeZoneInfo zone, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(GameDaysCacheKey, out IReadOnlyList<GameDay>? cached) && cached is not null)
        {
            return cached;
        }

        // Time zones cannot be converted in SQL, so bucket the (small) schedule into days in memory.
        var schedule = await _dbContext.Games
            .AsNoTracking()
            .Select(g => new
            {
                g.GameTime,
                g.TournamentId,
                TournamentName = g.Tournament.Name,
                TournamentLogo = g.Tournament.Logo,
            })
            .ToListAsync(cancellationToken);

        var days = schedule
            .GroupBy(g => (Date: ToEasternDate(g.GameTime, zone), g.TournamentId, g.TournamentName, g.TournamentLogo))
            .Select(group => new GameDay(group.Key.Date, group.Key.TournamentId, group.Key.TournamentName, group.Key.TournamentLogo, group.Count()))
            .OrderBy(d => d.Date)
            .ThenBy(d => d.TournamentId)
            .ToList();
        _cache.Set(GameDaysCacheKey, (IReadOnlyList<GameDay>)days, GameDaysCacheDuration);
        return days;
    }

    static DateOnly ToEasternDate(DateTime gameTime, TimeZoneInfo zone)
    {
        var utc = gameTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(gameTime, DateTimeKind.Utc)
            : gameTime.ToUniversalTime();
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, zone));
    }

    // Today if it has games, else the next game day, else the latest past game day.
    static DateOnly PickDefaultDate(IReadOnlyList<DateOnly> gameDates, DateOnly today)
    {
        if (gameDates.Count == 0 || gameDates.Contains(today))
        {
            return today;
        }

        return FirstAfter(gameDates, today) ?? gameDates[^1];
    }

    static DateOnly? FirstAfter(IEnumerable<DateOnly> sortedDates, DateOnly date) => sortedDates
            .Where(d => d > date)
            .Select(d => (DateOnly?)d)
            .FirstOrDefault();

    public async Task UpdateStateAsync(UpdateGameStateCommand command, CancellationToken cancellationToken = default)
    {
        var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == command.GameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Game), command.GameId);

        var wasInProgress = game.GameState == GameState.InProgress;
        var wasCompleted = game.GameState == GameState.Completed;
        game.GameState = command.State;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Starting a game clears the previous game's injected player songs and (optionally) queues this
        // game's participating players' songs at the front of the shared rotation.
        if (command.State == GameState.InProgress && !wasInProgress)
        {
            var playerTrackIds = command.IncludePlayerSongs
                ? await _music.GetOrderedRequestTrackIdsAsync(game.Id, cancellationToken)
                : [];
            await _queue.StartGameAsync(game.TournamentId, playerTrackIds, cancellationToken);
        }

        // Ending a game finalizes its goals/penalties, so recompute the stat tables.
        if (command.State == GameState.Completed && !wasCompleted)
        {
            await TryRefreshStatsAsync(game.Id, cancellationToken);
        }
    }

    public async Task<int> AddGoalAsync(CreateGoalCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureGameExistsAsync(command.GameId, cancellationToken);

        var goal = new Goal
        {
            GameId = command.GameId,
            TeamId = command.TeamId,
            Period = command.Period,
            PeriodLabel = command.PeriodLabel,
            PeriodTimeRemaining = command.PeriodTimeRemaining,
            GoalPlayerId = command.GoalPlayerId,
            Assist1PlayerId = command.Assist1PlayerId,
            Assist2PlayerId = command.Assist2PlayerId,
            Notes = command.Notes,
            IsEmptyNetGoal = command.IsEmptyNetGoal,
        };
        _dbContext.Goals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await TryRefreshStatsAsync(command.GameId, cancellationToken);
        return goal.Id;
    }

    public async Task UpdateGoalAsync(UpdateGoalCommand command, CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == command.GoalId && g.GameId == command.GameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Goal), command.GoalId);

        goal.TeamId = command.TeamId;
        goal.Period = command.Period;
        goal.PeriodLabel = command.PeriodLabel;
        goal.PeriodTimeRemaining = command.PeriodTimeRemaining;
        goal.GoalPlayerId = command.GoalPlayerId;
        goal.Assist1PlayerId = command.Assist1PlayerId;
        goal.Assist2PlayerId = command.Assist2PlayerId;
        goal.Notes = command.Notes;
        goal.IsEmptyNetGoal = command.IsEmptyNetGoal;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await TryRefreshStatsAsync(command.GameId, cancellationToken);
    }

    public async Task DeleteGoalAsync(int gameId, int goalId, CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.GameId == gameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Goal), goalId);

        _dbContext.Goals.Remove(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await TryRefreshStatsAsync(gameId, cancellationToken);
    }

    public async Task<int> AddPenaltyAsync(CreatePenaltyCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureGameExistsAsync(command.GameId, cancellationToken);

        var penalty = new Penalty
        {
            GameId = command.GameId,
            TeamId = command.TeamId,
            Period = command.Period,
            PeriodLabel = command.PeriodLabel,
            PeriodTimeRemaining = command.PeriodTimeRemaining,
            PlayerId = command.PlayerId,
            InfractionName = command.InfractionName,
            DurationMinutes = command.DurationMinutes,
            Notes = command.Notes,
        };
        _dbContext.Penalties.Add(penalty);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await TryRefreshStatsAsync(command.GameId, cancellationToken);
        return penalty.Id;
    }

    public async Task UpdatePenaltyAsync(UpdatePenaltyCommand command, CancellationToken cancellationToken = default)
    {
        var penalty = await _dbContext.Penalties
            .FirstOrDefaultAsync(p => p.Id == command.PenaltyId && p.GameId == command.GameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Penalty), command.PenaltyId);

        penalty.TeamId = command.TeamId;
        penalty.Period = command.Period;
        penalty.PeriodLabel = command.PeriodLabel;
        penalty.PeriodTimeRemaining = command.PeriodTimeRemaining;
        penalty.PlayerId = command.PlayerId;
        penalty.InfractionName = command.InfractionName;
        penalty.DurationMinutes = command.DurationMinutes;
        penalty.Notes = command.Notes;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await TryRefreshStatsAsync(command.GameId, cancellationToken);
    }

    public async Task DeletePenaltyAsync(int gameId, int penaltyId, CancellationToken cancellationToken = default)
    {
        var penalty = await _dbContext.Penalties
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.GameId == gameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Penalty), penaltyId);

        _dbContext.Penalties.Remove(penalty);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await TryRefreshStatsAsync(gameId, cancellationToken);
    }

    public async Task SetStarsAsync(SetGameStarsCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureGameExistsAsync(command.GameId, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existing = await _dbContext.GameStars
            .Where(s => s.GameId == command.GameId)
            .ToListAsync(cancellationToken);
        _dbContext.GameStars.RemoveRange(existing);

        var newStars = new List<GameStar>();
        if (command.FirstStarPlayerId.HasValue)
        {
            newStars.Add(new GameStar { GameId = command.GameId, StarRank = 1, PlayerId = command.FirstStarPlayerId.Value });
        }

        if (command.SecondStarPlayerId.HasValue)
        {
            newStars.Add(new GameStar { GameId = command.GameId, StarRank = 2, PlayerId = command.SecondStarPlayerId.Value });
        }

        if (command.ThirdStarPlayerId.HasValue)
        {
            newStars.Add(new GameStar { GameId = command.GameId, StarRank = 3, PlayerId = command.ThirdStarPlayerId.Value });
        }

        if (newStars.Count > 0)
        {
            _dbContext.GameStars.AddRange(newStars);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    async Task EnsureGameExistsAsync(int gameId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Games.AnyAsync(g => g.Id == gameId, cancellationToken);
        if (!exists)
        {
            throw new EntityNotFoundException(nameof(Game), gameId);
        }
    }

    // Recompute the stat tables after a change to game data. Best-effort: a refresh failure must not
    // fail the write that triggered it (the stats can also be refreshed manually from the Admin app).
    async Task TryRefreshStatsAsync(int gameId, CancellationToken cancellationToken)
    {
        try
        {
            await _stats.RefreshAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh stats after updating game {GameId}", gameId);
        }
    }
}