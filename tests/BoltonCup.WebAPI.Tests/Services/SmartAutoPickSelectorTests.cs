using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class SmartAutoPickSelectorTests
{
    private static AutoPickCandidate Skater(int id, double rank, string position, bool fluid = false) =>
        new(id, rank, position, fluid);

    private static AutoPickCandidate Forward(int id, double rank, bool fluid = false) =>
        Skater(id, rank, Position.Forward, fluid);

    private static AutoPickCandidate Defense(int id, double rank, bool fluid = false) =>
        Skater(id, rank, Position.Defense, fluid);

    private static AutoPickCandidate Goalie(int id, double rank) =>
        new(id, rank, Position.Goalie, false);

    private static List<RosteredPlayer> Roster(int forwards, int defense, int goalies = 0, int fluid = 0)
    {
        var roster = new List<RosteredPlayer>();
        for (var i = 0; i < forwards; i++) roster.Add(new RosteredPlayer(Position.Forward, false));
        for (var i = 0; i < defense; i++) roster.Add(new RosteredPlayer(Position.Defense, false));
        for (var i = 0; i < goalies; i++) roster.Add(new RosteredPlayer(Position.Goalie, false));
        for (var i = 0; i < fluid; i++) roster.Add(new RosteredPlayer(Position.Forward, true));
        return roster;
    }

    [Fact]
    public void Select_WhenRosterHeavilyForward_PicksBestDefense()
    {
        var available = new[]
        {
            Forward(1, 1),
            Forward(2, 2),
            Defense(3, 8),
            Defense(4, 9),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 7, defense: 0), remainingPicks: 9);

        chosen!.Value.PlayerId.Should().Be(3);
    }

    [Fact]
    public void Select_WhenForwardIsFarSuperior_OverridesModerateDefenseNeed()
    {
        // 9F/4D -> needD = 2 (bonus 4). A forward 10 ranks better than the best defense still wins.
        var available = new[]
        {
            Forward(1, 1),
            Defense(2, 12),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 9, defense: 4), remainingPicks: 6);

        chosen!.Value.PlayerId.Should().Be(1);
    }

    [Fact]
    public void Select_WhenRosterBalanced_PicksOverallBestSkater()
    {
        var available = new[]
        {
            Defense(1, 5),
            Forward(2, 3),
            Defense(3, 7),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 8, defense: 5), remainingPicks: 3);

        chosen!.Value.PlayerId.Should().Be(2);
    }

    [Fact]
    public void Select_WhenNoGoalieAndFinalSlot_PicksBestGoalie()
    {
        var available = new[]
        {
            Forward(1, 1),
            Goalie(2, 30),
            Goalie(3, 25),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 9, defense: 6), remainingPicks: 1);

        chosen!.Value.PlayerId.Should().Be(3);
    }

    [Fact]
    public void Select_WhenNoGoalieButSlotsRemain_NeverPicksGoalie()
    {
        var available = new[]
        {
            Goalie(1, 1),
            Forward(2, 5),
            Defense(3, 6),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 5, defense: 5), remainingPicks: 5);

        chosen!.Value.Position.Should().NotBe(Position.Goalie);
    }

    [Fact]
    public void Select_WhenTeamAlreadyHasGoalie_ExcludesGoalie()
    {
        var available = new[]
        {
            Goalie(1, 1),
            Forward(2, 5),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 8, defense: 6, goalies: 1), remainingPicks: 1);

        chosen!.Value.PlayerId.Should().Be(2);
    }

    [Fact]
    public void Select_WhenFluidCandidate_FillsMoreNeededSide()
    {
        // 9F/0D -> defense is the deep need. A fluid forward gets the defense bonus and beats a slightly
        // better-ranked dedicated forward.
        var available = new[]
        {
            Forward(1, 4),
            Forward(2, 6, fluid: true),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 9, defense: 0), remainingPicks: 6);

        chosen!.Value.PlayerId.Should().Be(2);
    }

    [Fact]
    public void Select_WhenFluidPlayerRostered_CountsTowardShortSide()
    {
        // 8 forwards + 1 fluid -> fluid fills defense (deeper deficit), so effective 8F/1D.
        // needD = 5 (bonus 10), needF = 1 (bonus 2): defense is strongly favored.
        var available = new[]
        {
            Forward(1, 1),
            Defense(2, 8),
        };

        var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 8, defense: 0, fluid: 1), remainingPicks: 6);

        chosen!.Value.PlayerId.Should().Be(2);
    }

    [Fact]
    public void Select_WhenNoCandidates_ReturnsNull()
    {
        var chosen = SmartAutoPickSelector.Select(Array.Empty<AutoPickCandidate>(), Roster(0, 0), remainingPicks: 5);

        chosen.Should().BeNull();
    }

    [Fact]
    public void Select_WithRandom_ProducesVariedResultsAcrossRuns()
    {
        // Closely-ranked skaters: noise should occasionally flip which one is chosen.
        var available = new[]
        {
            Forward(1, 3),
            Forward(2, 4),
            Defense(3, 3),
            Defense(4, 4),
        };

        var random = new Random(12345);
        var chosenIds = new HashSet<int>();
        for (var i = 0; i < 200; i++)
        {
            var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 4, defense: 4), remainingPicks: 7, random);
            chosenIds.Add(chosen!.Value.PlayerId);
        }

        chosenIds.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Select_WithRandom_StillPicksClearlySuperiorPlayer()
    {
        // Rank gap far exceeds NoiseMagnitude, so the top player must always win despite noise.
        var available = new[]
        {
            Forward(1, 1),
            Forward(2, 50),
            Defense(3, 51),
        };

        var random = new Random(999);
        for (var i = 0; i < 200; i++)
        {
            var chosen = SmartAutoPickSelector.Select(available, Roster(forwards: 4, defense: 4), remainingPicks: 7, random);
            chosen!.Value.PlayerId.Should().Be(1);
        }
    }
}
