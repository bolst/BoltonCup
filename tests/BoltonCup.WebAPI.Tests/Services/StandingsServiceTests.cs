using BoltonCup.Core;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class StandingsServiceTests
{
    private static Team Team(int id, string name) => new()
    {
        Id = id,
        Name = name,
        NameShort = name,
        Abbreviation = name,
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#FFFFFF",
    };

    private static Goal Goal(int gameId, int teamId, int period) => new()
    {
        GameId = gameId,
        TeamId = teamId,
        Period = period,
        PeriodLabel = period >= 4 ? "OT" : period.ToString(),
        PeriodTimeRemaining = TimeSpan.Zero,
        GoalPlayerId = 1,
    };

    private static Game Game(
        int id,
        int homeId,
        int awayId,
        int homeGoals,
        int awayGoals,
        GameType type = GameType.RoundRobin,
        bool otSo = false,
        GameState state = GameState.Completed)
    {
        var goals = new List<Goal>();
        for (var i = 0; i < homeGoals; i++) goals.Add(Goal(id, homeId, period: 1));
        for (var i = 0; i < awayGoals; i++) goals.Add(Goal(id, awayId, period: 1));
        if (otSo && goals.Count > 0) goals[^1].Period = 4;

        return new Game
        {
            Id = id,
            TournamentId = 1,
            GameTime = new DateTime(2026, 1, 1),
            HomeTeamId = homeId,
            AwayTeamId = awayId,
            GameType = type,
            GameState = state,
            Goals = goals,
        };
    }

    private static StandingRow Row(IReadOnlyList<StandingRow> rows, int teamId) =>
        rows.Single(r => r.TeamId == teamId);

    [Fact]
    public void RegulationWin_AwardsTwoToWinnerAndZeroToLoser()
    {
        var teams = new[] { Team(1, "A"), Team(2, "B") };
        var games = new[] { Game(1, homeId: 1, awayId: 2, homeGoals: 3, awayGoals: 1) };

        var rows = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);

        var winner = Row(rows, 1);
        winner.Points.Should().Be(2);
        winner.Wins.Should().Be(1);
        winner.RegulationWins.Should().Be(1);

        var loser = Row(rows, 2);
        loser.Points.Should().Be(0);
        loser.Losses.Should().Be(1);
        loser.OtSoLosses.Should().Be(0);
    }

    [Fact]
    public void Tie_AwardsOnePointEach()
    {
        var teams = new[] { Team(1, "A"), Team(2, "B") };
        var games = new[] { Game(1, homeId: 1, awayId: 2, homeGoals: 2, awayGoals: 2) };

        var rows = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);

        Row(rows, 1).Points.Should().Be(1);
        Row(rows, 1).Ties.Should().Be(1);
        Row(rows, 2).Points.Should().Be(1);
        Row(rows, 2).Ties.Should().Be(1);
    }

    [Fact]
    public void OtSoWin_AwardsTwoToWinnerAndOneToLoser()
    {
        var teams = new[] { Team(1, "A"), Team(2, "B") };
        var games = new[] { Game(1, homeId: 1, awayId: 2, homeGoals: 4, awayGoals: 3, otSo: true) };

        var rows = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);

        var winner = Row(rows, 1);
        winner.Points.Should().Be(2);
        winner.Wins.Should().Be(1);
        winner.RegulationWins.Should().Be(0); // OT/SO win is not a regulation win

        var loser = Row(rows, 2);
        loser.Points.Should().Be(1);
        loser.OtSoLosses.Should().Be(1);
        loser.Losses.Should().Be(0);
    }

    [Fact]
    public void StageFiltering_OnlyCountsGamesInRequestedStage()
    {
        var teams = new[] { Team(1, "A"), Team(2, "B") };
        var games = new[]
        {
            Game(1, homeId: 1, awayId: 2, homeGoals: 3, awayGoals: 0, type: GameType.RoundRobin),
            Game(2, homeId: 2, awayId: 1, homeGoals: 5, awayGoals: 0, type: GameType.Finals),
        };

        var roundRobin = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);
        Row(roundRobin, 1).GamesPlayed.Should().Be(1);
        Row(roundRobin, 1).Points.Should().Be(2);
        Row(roundRobin, 2).Points.Should().Be(0);

        var playoffs = StandingsService.Compute(teams, games, StandingsStage.Playoffs, StandingsRules.Default);
        Row(playoffs, 2).GamesPlayed.Should().Be(1);
        Row(playoffs, 2).Points.Should().Be(2);
        Row(playoffs, 1).Points.Should().Be(0);
    }

    [Fact]
    public void TeamsWithNoGames_AppearWithZeros()
    {
        var teams = new[] { Team(1, "A"), Team(2, "B") };

        var rows = StandingsService.Compute(teams, [], StandingsStage.RoundRobin, StandingsRules.Default);

        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(r => r.GamesPlayed == 0 && r.Points == 0);
    }

    [Fact]
    public void HeadToHead_BreaksTieBeforeGoalDifferential()
    {
        // A and B finish level on points. By goal differential B is ahead, but A won the
        // head-to-head meeting, so A must be ranked above B.
        var teams = new[] { Team(1, "A"), Team(2, "B"), Team(3, "X"), Team(4, "Y") };
        var games = new[]
        {
            Game(1, homeId: 1, awayId: 2, homeGoals: 1, awayGoals: 0), // A beats B (head-to-head)
            Game(2, homeId: 4, awayId: 1, homeGoals: 5, awayGoals: 0), // A blown out by Y (hurts A diff)
            Game(3, homeId: 2, awayId: 3, homeGoals: 5, awayGoals: 0), // B routs X (boosts B diff)
        };

        var rows = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);

        Row(rows, 1).Points.Should().Be(2);
        Row(rows, 2).Points.Should().Be(2);
        Row(rows, 1).GoalDifferential.Should().BeLessThan(Row(rows, 2).GoalDifferential);
        Row(rows, 1).Rank.Should().BeLessThan(Row(rows, 2).Rank); // head-to-head wins out
    }

    [Fact]
    public void RegulationWins_BreakTieWhenHeadToHeadEqual()
    {
        // A and B never played each other and are level on points; A has a regulation win,
        // B's win came in OT, so A ranks higher on regulation wins.
        var teams = new[] { Team(1, "A"), Team(2, "B"), Team(3, "X"), Team(4, "Y") };
        var games = new[]
        {
            Game(1, homeId: 1, awayId: 3, homeGoals: 2, awayGoals: 0),               // A reg win (2 pts)
            Game(2, homeId: 2, awayId: 4, homeGoals: 2, awayGoals: 1, otSo: true),    // B OT win (2 pts)
        };

        var rows = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);

        Row(rows, 1).Points.Should().Be(2);
        Row(rows, 2).Points.Should().Be(2);
        Row(rows, 1).RegulationWins.Should().Be(1);
        Row(rows, 2).RegulationWins.Should().Be(0);
        Row(rows, 1).Rank.Should().BeLessThan(Row(rows, 2).Rank);
    }
}
