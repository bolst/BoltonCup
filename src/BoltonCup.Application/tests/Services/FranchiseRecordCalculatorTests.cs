using BoltonCup.Application.Services;
using BoltonCup.Core;
using FluentAssertions;
using Xunit;
using static BoltonCup.Application.Tests.Helpers.TestData;

namespace BoltonCup.Application.Tests.Services;

public class FranchiseRecordCalculatorTests
{
    const int FranchiseId = 1;
    const int OtherFranchiseId = 2;

    [Fact]
    public void Record_SumsAllCompletedGamesAcrossSeasons_AllGameTypes()
    {
        // Team 10 (season 1) and team 20 (season 2) belong to the franchise; 90 and 91 are opponents.
        var teams = new[] { Team(10, FranchiseId, 1), Team(20, FranchiseId, 2) };
        var games = new[]
        {
            Game(1, homeId: 10, awayId: 90, homeGoals: 3, awayGoals: 1, type: GameType.RoundRobin, tournamentId: 1),
            Game(2, homeId: 90, awayId: 10, homeGoals: 2, awayGoals: 0, type: GameType.SemiFinals, tournamentId: 1),
            Game(3, homeId: 20, awayId: 91, homeGoals: 4, awayGoals: 2, type: GameType.Finals, tournamentId: 2),
            Game(4, homeId: 91, awayId: 20, homeGoals: 1, awayGoals: 1, type: GameType.ThirdPlaceFinals, tournamentId: 2),
        };

        var records = FranchiseRecordCalculator.Compute(teams, games);

        records.AllTime.Should().Be(new FranchiseRecord(GamesPlayed: 4, Wins: 2, Losses: 1, Ties: 1, GoalsFor: 8, GoalsAgainst: 6));
        records.IntraFranchiseGames.Should().Be(0);
    }

    [Fact]
    public void Record_ExcludesIncompleteAndPlaceholderGames()
    {
        var teams = new[] { Team(10, FranchiseId, 1) };
        var games = new[]
        {
            Game(1, homeId: 10, awayId: 90, homeGoals: 3, awayGoals: 1),
            Game(2, homeId: 10, awayId: 90, homeGoals: 5, awayGoals: 0, state: GameState.InProgress),
            Game(3, homeId: 10, awayId: 90, homeGoals: 0, awayGoals: 0, state: GameState.Pending),
            Game(4, homeId: 10, awayId: null, homeGoals: 2, awayGoals: 0),
        };

        var records = FranchiseRecordCalculator.Compute(teams, games);

        records.AllTime.Should().Be(new FranchiseRecord(1, 1, 0, 0, 3, 1));
    }

    [Fact]
    public void Record_OtSoLossCountsAsLoss()
    {
        var teams = new[] { Team(10, FranchiseId, 1) };
        var games = new[] { Game(1, homeId: 90, awayId: 10, homeGoals: 3, awayGoals: 2, otSo: true) };

        var records = FranchiseRecordCalculator.Compute(teams, games);

        records.AllTime.Should().Be(new FranchiseRecord(1, 0, 1, 0, 2, 3));
    }

    [Fact]
    public void Record_TieCountsAsTie()
    {
        var teams = new[] { Team(10, FranchiseId, 1) };
        var games = new[]
        {
            Game(1, homeId: 10, awayId: 90, homeGoals: 2, awayGoals: 2),
            Game(2, homeId: 91, awayId: 10, homeGoals: 0, awayGoals: 0),
        };

        var records = FranchiseRecordCalculator.Compute(teams, games);

        records.AllTime.Should().Be(new FranchiseRecord(2, 0, 0, 2, 2, 2));
    }

    [Fact]
    public void Record_IntraFranchiseGame_CountsBothSides()
    {
        // Two teams of the same franchise meet in one tournament.
        var teams = new[] { Team(10, FranchiseId, 1), Team(11, FranchiseId, 1) };
        var games = new[] { Game(1, homeId: 10, awayId: 11, homeGoals: 4, awayGoals: 1) };

        var records = FranchiseRecordCalculator.Compute(teams, games);

        records.AllTime.Should().Be(new FranchiseRecord(GamesPlayed: 2, Wins: 1, Losses: 1, Ties: 0, GoalsFor: 5, GoalsAgainst: 5));
        records.IntraFranchiseGames.Should().Be(1);
    }

    [Fact]
    public void Record_SeasonRowsSumToAllTime()
    {
        var teams = new[] { Team(10, FranchiseId, 1), Team(11, FranchiseId, 1), Team(20, FranchiseId, 2) };
        var games = new[]
        {
            Game(1, homeId: 10, awayId: 11, homeGoals: 2, awayGoals: 2),
            Game(2, homeId: 10, awayId: 90, homeGoals: 1, awayGoals: 0),
            Game(3, homeId: 90, awayId: 11, homeGoals: 3, awayGoals: 2, otSo: true),
            Game(4, homeId: 20, awayId: 91, homeGoals: 5, awayGoals: 1, tournamentId: 2),
        };

        var records = FranchiseRecordCalculator.Compute(teams, games);

        var summed = records.ByTeam.Values.Aggregate(FranchiseRecord.Empty, (sum, r) => sum.Add(r));
        summed.Should().Be(records.AllTime);
        records.ByTeam[10].Should().Be(new FranchiseRecord(2, 1, 0, 1, 3, 2));
        records.ByTeam[11].Should().Be(new FranchiseRecord(2, 0, 1, 1, 4, 5));
        records.ByTeam[20].Should().Be(new FranchiseRecord(1, 1, 0, 0, 5, 1));
    }

    [Fact]
    public void Record_AgreesWithStandings_OnSameFixture()
    {
        var teams = new[]
        {
            Team(1, FranchiseId, 1, "A"),
            Team(2, OtherFranchiseId, 1, "B"),
            Team(3, 3, 1, "C"),
        };
        var games = new[]
        {
            Game(1, homeId: 1, awayId: 2, homeGoals: 3, awayGoals: 1),
            Game(2, homeId: 2, awayId: 3, homeGoals: 2, awayGoals: 2),
            Game(3, homeId: 3, awayId: 1, homeGoals: 4, awayGoals: 3, otSo: true),
            Game(4, homeId: 1, awayId: 3, homeGoals: 1, awayGoals: 0, type: GameType.SemiFinals),
            Game(5, homeId: 1, awayId: 2, homeGoals: 2, awayGoals: 5, type: GameType.Finals),
            Game(6, homeId: 2, awayId: 3, homeGoals: 9, awayGoals: 0, state: GameState.InProgress),
        };

        // ToStandingsStage maps every non-round-robin game type to Playoffs, so summing both stages covers all
        // game types the calculator counts.
        var roundRobin = StandingsService.Compute(teams, games, StandingsStage.RoundRobin, StandingsRules.Default);
        var playoffs = StandingsService.Compute(teams, games, StandingsStage.Playoffs, StandingsRules.Default);

        foreach (var team in teams)
        {
            var records = FranchiseRecordCalculator.Compute([team], games);
            var rr = roundRobin.Single(r => r.TeamId == team.Id);
            var po = playoffs.Single(r => r.TeamId == team.Id);

            records.AllTime.Should().Be(new FranchiseRecord(
                rr.GamesPlayed + po.GamesPlayed,
                rr.Wins + po.Wins,
                rr.Losses + rr.OtSoLosses + po.Losses + po.OtSoLosses,
                rr.Ties + po.Ties,
                rr.GoalsFor + po.GoalsFor,
                rr.GoalsAgainst + po.GoalsAgainst), $"team {team.Id} should agree with standings");
        }
    }
}