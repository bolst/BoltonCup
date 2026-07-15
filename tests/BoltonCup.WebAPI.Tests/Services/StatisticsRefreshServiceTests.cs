using BoltonCup.Core;
using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class StatisticsRefreshServiceTests
{
    private static IDbContextFactory<BoltonCupDbContext> BuildFactory(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<BoltonCupDbContext>(o => o
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        return services.BuildServiceProvider().GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
    }

    private static Team Team(int id, string name) => new()
    {
        Id = id,
        Name = name,
        NameShort = name,
        Abbreviation = name,
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#FFFFFF",
    };

    private static Account Account(int id) => new()
    {
        Id = id,
        FirstName = $"First{id}",
        LastName = $"Last{id}",
        Email = $"player{id}@example.com",
        Birthday = new DateTime(1990, 1, 1),
    };

    private static Player Player(int id, int teamId, string position) => new()
    {
        Id = id,
        TournamentId = 1,
        AccountId = id,
        TeamId = teamId,
        Position = position,
    };

    private static Goal Goal(int id, int gameId, int teamId, int scorerId, int? assistId = null) => new()
    {
        Id = id,
        GameId = gameId,
        TeamId = teamId,
        Period = 1,
        PeriodLabel = "1",
        PeriodTimeRemaining = TimeSpan.Zero,
        GoalPlayerId = scorerId,
        Assist1PlayerId = assistId,
    };

    private static async Task SeedAsync(IDbContextFactory<BoltonCupDbContext> factory)
    {
        await using var db = await factory.CreateDbContextAsync();

        db.Tournaments.Add(new Tournament { Id = 1, Name = "Cup", IsActive = true });
        db.Teams.AddRange(Team(1, "Home"), Team(2, "Away"));
        db.Accounts.AddRange(Account(1), Account(2), Account(3), Account(4), Account(5));
        db.Players.AddRange(
            Player(1, teamId: 1, Position.Forward),
            Player(2, teamId: 1, Position.Forward),
            Player(3, teamId: 1, Position.Goalie),
            Player(4, teamId: 2, Position.Forward),
            Player(5, teamId: 2, Position.Goalie));
        db.Games.Add(new Game
        {
            Id = 1,
            TournamentId = 1,
            GameTime = new DateTime(2026, 1, 1),
            HomeTeamId = 1,
            AwayTeamId = 2,
            GameType = GameType.RoundRobin,
            GameState = GameState.Completed,
        });
        // Home (team 1) scores twice, away (team 2) once. P1 scores both home goals, P2 assists one.
        db.Goals.AddRange(
            Goal(1, gameId: 1, teamId: 1, scorerId: 1, assistId: 2),
            Goal(2, gameId: 1, teamId: 1, scorerId: 1),
            Goal(3, gameId: 1, teamId: 2, scorerId: 4));
        db.Penalties.Add(new Penalty
        {
            Id = 1,
            GameId = 1,
            TeamId = 1,
            Period = 1,
            PeriodLabel = "1",
            PeriodTimeRemaining = TimeSpan.Zero,
            PlayerId = 1,
            InfractionName = "Tripping",
            DurationMinutes = 2,
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task RefreshAsync_ComputesSkaterGameLogs()
    {
        var factory = BuildFactory($"stats-skater-{Guid.NewGuid()}");
        await SeedAsync(factory);

        await new StatisticsRefreshService(factory).RefreshAsync();

        await using var db = await factory.CreateDbContextAsync();
        var skaters = await db.SkaterStats.AsNoTracking().ToListAsync();

        // Only the three non-goalie players get skater rows.
        skaters.Select(s => s.PlayerId).Should().BeEquivalentTo(new[] { 1, 2, 4 });

        var p1 = skaters.Single(s => s.PlayerId == 1);
        p1.Goals.Should().Be(2);
        p1.Assists.Should().Be(0);
        p1.Points.Should().Be(2);
        p1.PenaltyMinutes.Should().Be(2);
        p1.GamesPlayed.Should().Be(1);
        p1.TeamId.Should().Be(1);
        p1.OpponentId.Should().Be(2);
        p1.GameType.Should().Be("Round robin");

        var p2 = skaters.Single(s => s.PlayerId == 2);
        p2.Goals.Should().Be(0);
        p2.Assists.Should().Be(1);
        p2.Points.Should().Be(1);

        var p4 = skaters.Single(s => s.PlayerId == 4);
        p4.Goals.Should().Be(1);
        p4.TeamId.Should().Be(2);
        p4.OpponentId.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_ComputesGoalieGameLogs()
    {
        var factory = BuildFactory($"stats-goalie-{Guid.NewGuid()}");
        await SeedAsync(factory);

        await new StatisticsRefreshService(factory).RefreshAsync();

        await using var db = await factory.CreateDbContextAsync();
        var goalies = await db.GoalieStats.AsNoTracking().ToListAsync();

        goalies.Select(g => g.PlayerId).Should().BeEquivalentTo(new[] { 3, 5 });

        // Team 1 goalie: opponent (team 2) scored 1, own team scored 2 → a win, not a shutout.
        var homeGoalie = goalies.Single(g => g.PlayerId == 3);
        homeGoalie.GoalsAgainst.Should().Be(1);
        homeGoalie.Wins.Should().Be(1);
        homeGoalie.Shutouts.Should().Be(0);
        homeGoalie.GoalsAgainstAverage.Should().Be(1);

        // Team 2 goalie: opponent (team 1) scored 2, own team scored 1 → a loss.
        var awayGoalie = goalies.Single(g => g.PlayerId == 5);
        awayGoalie.GoalsAgainst.Should().Be(2);
        awayGoalie.Wins.Should().Be(0);
        awayGoalie.Shutouts.Should().Be(0);
    }

    [Fact]
    public async Task RefreshAsync_ExcludesEmptyNetGoalsFromGaa_ButNotGoalsAgainst()
    {
        var factory = BuildFactory($"stats-emptynet-{Guid.NewGuid()}");

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Tournaments.Add(new Tournament { Id = 1, Name = "Cup", IsActive = true });
            db.Teams.AddRange(Team(1, "Home"), Team(2, "Away"));
            db.Accounts.AddRange(Account(1), Account(2), Account(3), Account(4), Account(5));
            db.Players.AddRange(
                Player(1, teamId: 1, Position.Forward),
                Player(2, teamId: 1, Position.Forward),
                Player(3, teamId: 1, Position.Goalie),
                Player(4, teamId: 2, Position.Forward),
                Player(5, teamId: 2, Position.Goalie));
            db.Games.Add(new Game
            {
                Id = 1,
                TournamentId = 1,
                GameTime = new DateTime(2026, 1, 1),
                HomeTeamId = 1,
                AwayTeamId = 2,
                GameType = GameType.RoundRobin,
                GameState = GameState.Completed,
            });
            // Away (team 2) scores twice against the home goalie; the second is an empty-netter.
            var enGoal = Goal(2, gameId: 1, teamId: 2, scorerId: 4);
            enGoal.IsEmptyNetGoal = true;
            db.Goals.AddRange(
                Goal(1, gameId: 1, teamId: 2, scorerId: 4),
                enGoal);
            await db.SaveChangesAsync();
        }

        await new StatisticsRefreshService(factory).RefreshAsync();

        await using var read = await factory.CreateDbContextAsync();
        var homeGoalie = await read.GoalieStats.AsNoTracking().SingleAsync(g => g.PlayerId == 3);

        // Raw goals-against still counts the empty-netter (2), but GAA excludes it (1).
        homeGoalie.GoalsAgainst.Should().Be(2);
        homeGoalie.GoalsAgainstAverage.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_IsIdempotent_ReplacingExistingRows()
    {
        var factory = BuildFactory($"stats-idempotent-{Guid.NewGuid()}");
        await SeedAsync(factory);

        var service = new StatisticsRefreshService(factory);
        await service.RefreshAsync();
        await service.RefreshAsync();

        await using var db = await factory.CreateDbContextAsync();
        // Three skaters + two goalies, and running twice must not duplicate the (game, player) rows.
        (await db.SkaterStats.CountAsync()).Should().Be(3);
        (await db.GoalieStats.CountAsync()).Should().Be(2);
    }
}
