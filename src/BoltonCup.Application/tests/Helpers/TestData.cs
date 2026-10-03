using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Tests.Helpers;

static class TestData
{
    public static Franchise Franchise(int id, string? name = null) => new()
    {
        Id = id,
        Name = name ?? $"Franchise {id}",
        Slug = $"franchise-{id}",
        NameShort = name ?? $"F{id}",
        Abbreviation = $"F{id}",
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#FFFFFF",
    };

    public static Team Team(int id, int franchiseId, int? tournamentId, string? name = null) => new()
    {
        Id = id,
        FranchiseId = franchiseId,
        TournamentId = tournamentId,
        Name = name ?? $"Team {id}",
        NameShort = name ?? $"T{id}",
        Abbreviation = $"T{id}",
        PrimaryColorHex = "#111111",
        SecondaryColorHex = "#EEEEEE",
    };

    /// <summary>A game whose goals produce the given score; with <paramref name="otSo"/> the last goal is in period 4.</summary>
    public static Game Game(
        int id,
        int? homeId,
        int? awayId,
        int homeGoals,
        int awayGoals,
        GameType type = GameType.RoundRobin,
        bool otSo = false,
        GameState state = GameState.Completed,
        int tournamentId = 1)
    {
        var goals = new List<Goal>();
        for (var i = 0; i < homeGoals; i++)
        {
            goals.Add(Goal(id, homeId ?? 0, period: 1));
        }

        for (var i = 0; i < awayGoals; i++)
        {
            goals.Add(Goal(id, awayId ?? 0, period: 1));
        }

        if (otSo && goals.Count > 0)
        {
            goals[^1].Period = 4;
        }

        return new Game
        {
            Id = id,
            TournamentId = tournamentId,
            GameTime = new DateTime(2026, 1, 1),
            HomeTeamId = homeId,
            AwayTeamId = awayId,
            GameType = type,
            GameState = state,
            Goals = goals,
        };
    }

    static Goal Goal(int gameId, int teamId, int period) => new()
    {
        GameId = gameId,
        TeamId = teamId,
        Period = period,
        PeriodLabel = period >= 4 ? "OT" : period.ToString(),
        PeriodTimeRemaining = TimeSpan.Zero,
        GoalPlayerId = 1,
    };
}

/// <summary>An InMemory <see cref="IDbContextFactory{TContext}"/> whose contexts all share one database.</summary>
sealed class TestDbContextFactory(string databaseName) : IDbContextFactory<BoltonCupDbContext>
{
    readonly DbContextOptions<BoltonCupDbContext> _options = new DbContextOptionsBuilder<BoltonCupDbContext>()
        .UseInMemoryDatabase(databaseName)
        .Options;

    public TestDbContextFactory() : this($"test-{Guid.NewGuid()}")
    {
    }

    public BoltonCupDbContext CreateDbContext() => new(_options);
}