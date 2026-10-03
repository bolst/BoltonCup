using BoltonCup.Application.Services;
using BoltonCup.Core;
using FluentAssertions;
using Xunit;
using static BoltonCup.Application.Tests.Helpers.TestData;

namespace BoltonCup.Application.Tests.Services;

public class GameResultTests
{
    [Theory]
    [InlineData(GameState.Pending)]
    [InlineData(GameState.InProgress)]
    public void For_ReturnsNull_WhenNotCompleted(GameState state)
    {
        var game = Game(1, homeId: 1, awayId: 2, homeGoals: 2, awayGoals: 1, state: state);

        GameResult.For(game, 1).Should().BeNull();
    }

    [Fact]
    public void For_ReturnsNull_WhenTeamMissingOrPlaceholder()
    {
        var placeholder = Game(1, homeId: 1, awayId: null, homeGoals: 1, awayGoals: 0);
        var other = Game(2, homeId: 1, awayId: 2, homeGoals: 1, awayGoals: 0);

        GameResult.For(placeholder, 1).Should().BeNull();
        GameResult.For(other, 3).Should().BeNull();
    }

    [Fact]
    public void For_CountsGoalsPerSide()
    {
        var game = Game(1, homeId: 1, awayId: 2, homeGoals: 3, awayGoals: 1);

        GameResult.For(game, 1).Should().Be(new GameResult(3, 1, IsOtSo: false));
        GameResult.For(game, 2).Should().Be(new GameResult(1, 3, IsOtSo: false));
        GameResult.For(game, 1)!.Value.IsWin.Should().BeTrue();
        GameResult.For(game, 2)!.Value.IsLoss.Should().BeTrue();
    }

    [Fact]
    public void For_FlagsOtSo_WhenGoalInPeriod4OrLater()
    {
        var game = Game(1, homeId: 1, awayId: 2, homeGoals: 3, awayGoals: 2, otSo: true);

        GameResult.For(game, 1)!.Value.IsOtSo.Should().BeTrue();
        GameResult.For(game, 2)!.Value.IsOtSo.Should().BeTrue();
    }

    [Fact]
    public void For_CompletedZeroZero_IsTie()
    {
        // A forfeit is recorded as a completed game with no goals.
        var game = Game(1, homeId: 1, awayId: 2, homeGoals: 0, awayGoals: 0);

        var result = GameResult.For(game, 1);

        result.Should().NotBeNull();
        result!.Value.IsTie.Should().BeTrue();
        result.Value.IsOtSo.Should().BeFalse();
    }
}