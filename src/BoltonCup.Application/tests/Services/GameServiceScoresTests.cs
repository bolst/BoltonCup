using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class GameServiceScoresTests
{
    // Noon Eastern (EDT, UTC-4) on Thursday, October 15, 2026.
    static readonly DateTimeOffset Now = new(2026, 10, 15, 16, 0, 0, TimeSpan.Zero);

    static readonly DateOnly Oct1 = new(2026, 10, 1);
    static readonly DateOnly Oct5 = new(2026, 10, 5);
    static readonly DateOnly Oct10 = new(2026, 10, 10);
    static readonly DateOnly Oct14 = new(2026, 10, 14);
    static readonly DateOnly Oct15 = new(2026, 10, 15);
    static readonly DateOnly Oct16 = new(2026, 10, 16);
    static readonly DateOnly Oct17 = new(2026, 10, 17);
    static readonly DateOnly Oct20 = new(2026, 10, 20);

    int _nextGameId = 1;

    static BoltonCupDbContext NewContext() =>
        new(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseInMemoryDatabase($"game-scores-{Guid.NewGuid()}")
            .Options);

    static GameService NewService(BoltonCupDbContext db, IMemoryCache? cache = null) => new(
        db,
        Mock.Of<IGlobalMusicQueue>(),
        Mock.Of<IMusicLibraryService>(),
        Mock.Of<IStatisticsRefreshService>(),
        new FixedTimeProvider(Now),
        cache ?? new MemoryCache(new MemoryCacheOptions()),
        NullLogger<GameService>.Instance);

    static Tournament Tournament(int id) => new()
    {
        Id = id,
        Name = $"Bolton Cup {id}",
        Logo = $"logos/{id}.png",
    };

    // 19:00 Eastern (EDT) on the given day.
    static DateTime EveningOf(DateOnly date) => date.ToDateTime(new TimeOnly(23, 0), DateTimeKind.Utc);

    Game Game(DateTime gameTime, int tournamentId = 1, GameState state = GameState.Pending) => new()
    {
        Id = _nextGameId++,
        TournamentId = tournamentId,
        GameTime = gameTime,
        GameState = state,
    };

    async Task<BoltonCupDbContext> SeedAsync(params Game[] games)
    {
        var db = NewContext();
        db.Tournaments.AddRange(Tournament(1), Tournament(2));
        db.Games.AddRange(games);
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task GetScoresAsync_DefaultsToToday_WhenTodayHasGames()
    {
        await using var db = await SeedAsync(Game(EveningOf(Oct14)), Game(EveningOf(Oct15)), Game(EveningOf(Oct17)));

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(null));

        scores.Date.Should().Be(Oct15);
        scores.PreviousDate.Should().Be(Oct14);
        scores.NextDate.Should().Be(Oct17);
        scores.Games.Should().ContainSingle();
    }

    [Fact]
    public async Task GetScoresAsync_DefaultsToNextGameDay_WhenTodayHasNoGames()
    {
        await using var db = await SeedAsync(Game(EveningOf(Oct10)), Game(EveningOf(Oct17)), Game(EveningOf(Oct20)));

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(null));

        scores.Date.Should().Be(Oct17);
        scores.PreviousDate.Should().Be(Oct10);
        scores.NextDate.Should().Be(Oct20);
    }

    [Fact]
    public async Task GetScoresAsync_ReusesCachedGameDays_WhenScheduleChanges()
    {
        await using var db = await SeedAsync(Game(EveningOf(Oct15)));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        await NewService(db, cache).GetScoresAsync(new GetScoresQuery(Oct15));

        db.Games.Add(Game(EveningOf(Oct17)));
        await db.SaveChangesAsync();
        var scores = await NewService(db, cache).GetScoresAsync(new GetScoresQuery(Oct15));

        scores.Days.Select(d => d.Date).Should().Equal(Oct15);
        scores.NextDate.Should().BeNull();
    }

    [Fact]
    public async Task GetScoresAsync_DefaultsToLatestPastGameDay_WhenNoGamesAreUpcoming()
    {
        await using var db = await SeedAsync(Game(EveningOf(Oct1)), Game(EveningOf(Oct5)));

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(null));

        scores.Date.Should().Be(Oct5);
        scores.PreviousDate.Should().Be(Oct1);
        scores.NextDate.Should().BeNull();
    }

    [Fact]
    public async Task GetScoresAsync_DefaultsToToday_WhenThereAreNoGames()
    {
        await using var db = await SeedAsync();

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(null));

        scores.Date.Should().Be(Oct15);
        scores.PreviousDate.Should().BeNull();
        scores.NextDate.Should().BeNull();
        scores.Days.Should().BeEmpty();
        scores.Games.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScoresAsync_ReturnsRequestedDay_InStartTimeOrder()
    {
        var late = Game(EveningOf(Oct14));
        var early = Game(EveningOf(Oct14).AddHours(-2));
        await using var db = await SeedAsync(late, early, Game(EveningOf(Oct15)));

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(Oct14));

        scores.Date.Should().Be(Oct14);
        scores.PreviousDate.Should().BeNull();
        scores.NextDate.Should().Be(Oct15);
        scores.Games.Select(g => g.Id).Should().Equal(early.Id, late.Id);
    }

    [Fact]
    public async Task GetScoresAsync_GroupsDaysByTournament()
    {
        await using var db = await SeedAsync(
            Game(EveningOf(Oct14), tournamentId: 1),
            Game(EveningOf(Oct14).AddHours(-1), tournamentId: 1),
            Game(EveningOf(Oct14), tournamentId: 2),
            Game(EveningOf(Oct15), tournamentId: 1));

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(Oct14));

        scores.Days.Should().Equal(
            new GameDay(Oct14, 1, "Bolton Cup 1", "logos/1.png", 2),
            new GameDay(Oct14, 2, "Bolton Cup 2", "logos/2.png", 1),
            new GameDay(Oct15, 1, "Bolton Cup 1", "logos/1.png", 1));
        scores.Games.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetScoresAsync_BucketsLateGamesOnTheirEasternDate()
    {
        // 23:30 EDT on October 15 is 03:30 UTC on October 16.
        var lateGame = Game(new DateTime(2026, 10, 16, 3, 30, 0, DateTimeKind.Utc));
        await using var db = await SeedAsync(lateGame);
        var service = NewService(db);

        var onEasternDay = await service.GetScoresAsync(new GetScoresQuery(Oct15));
        var onUtcDay = await service.GetScoresAsync(new GetScoresQuery(Oct16));

        onEasternDay.Days.Should().ContainSingle().Which.Date.Should().Be(Oct15);
        onEasternDay.Games.Should().ContainSingle().Which.Id.Should().Be(lateGame.Id);
        onUtcDay.Games.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScoresAsync_IncludesGoalsWithPlayers()
    {
        var game = Game(EveningOf(Oct15), state: GameState.Completed);
        var account = new Account
        {
            Id = 1,
            FirstName = "Wayne",
            LastName = "Gretzky",
            Email = "wayne@example.com",
            Birthday = new DateTime(1961, 1, 26),
        };
        var player = new Player { Id = 1, TournamentId = 1, AccountId = account.Id, Account = account };
        game.Goals.Add(new Goal
        {
            GameId = game.Id,
            TeamId = 1,
            Period = 1,
            PeriodLabel = "1st",
            PeriodTimeRemaining = TimeSpan.FromMinutes(5),
            GoalPlayerId = player.Id,
        });
        await using var db = NewContext();
        db.Tournaments.Add(Tournament(1));
        db.Players.Add(player);
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var scores = await NewService(db).GetScoresAsync(new GetScoresQuery(Oct15));

        var goal = scores.Games.Should().ContainSingle().Which.Goals.Should().ContainSingle().Subject;
        goal.Scorer.Account.LastName.Should().Be("Gretzky");
    }

    [Theory]
    [InlineData(-30, GameState.Pending, GameScoreStatus.Upcoming)]
    [InlineData(120, GameState.Pending, GameScoreStatus.Upcoming)]
    [InlineData(-30, GameState.InProgress, GameScoreStatus.Live)]
    [InlineData(120, GameState.InProgress, GameScoreStatus.Live)]
    [InlineData(30, GameState.Completed, GameScoreStatus.Final)]
    public void GetScoreStatus_FollowsGameState_RegardlessOfStartTime(int minutesSinceStart, GameState state, GameScoreStatus expected)
    {
        var game = Game(Now.UtcDateTime.AddMinutes(-minutesSinceStart), state: state);

        game.GetScoreStatus().Should().Be(expected);
    }

    sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}