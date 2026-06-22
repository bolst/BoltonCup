using BoltonCup.Core;
using BoltonCup.Core.Values;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class RosterValidatorTests
{
    private static readonly RosterValidator Validator = new();

    private static List<Player> Roster(int forwards, int defense, int goalies)
    {
        var players = new List<Player>();
        var id = 1;
        for (var i = 0; i < forwards; i++)
            players.Add(new Player { Id = id++, AccountId = id, TournamentId = 1, Position = Position.Forward });
        for (var i = 0; i < defense; i++)
            players.Add(new Player { Id = id++, AccountId = id, TournamentId = 1, Position = Position.Defense });
        for (var i = 0; i < goalies; i++)
            players.Add(new Player { Id = id++, AccountId = id, TournamentId = 1, Position = Position.Goalie });
        return players;
    }

    [Fact]
    public void NullLimits_AlwaysValid()
    {
        var result = Validator.Validate(Roster(20, 10, 5), skaterLimit: null, goalieLimit: null);
        result.IsValid.Should().BeTrue();
        result.Reasons.Should().BeEmpty();
    }

    [Fact]
    public void UnderLimits_IsValid()
    {
        var result = Validator.Validate(Roster(6, 4, 1), skaterLimit: 12, goalieLimit: 2);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AtLimits_IsValid()
    {
        var result = Validator.Validate(Roster(8, 4, 2), skaterLimit: 12, goalieLimit: 2);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void TooManySkaters_IsInvalid()
    {
        var result = Validator.Validate(Roster(10, 4, 1), skaterLimit: 12, goalieLimit: 2);
        result.IsValid.Should().BeFalse();
        result.Reasons.Should().ContainSingle().Which.Should().Contain("skaters");
    }

    [Fact]
    public void TooManyGoalies_IsInvalid()
    {
        var result = Validator.Validate(Roster(6, 4, 3), skaterLimit: 12, goalieLimit: 2);
        result.IsValid.Should().BeFalse();
        result.Reasons.Should().ContainSingle().Which.Should().Contain("goalies");
    }

    [Fact]
    public void BothOverLimit_ReportsBothReasons()
    {
        var result = Validator.Validate(Roster(10, 4, 3), skaterLimit: 12, goalieLimit: 2);
        result.IsValid.Should().BeFalse();
        result.Reasons.Should().HaveCount(2);
    }

    [Fact]
    public void OnlyGoalieLimitSet_SkatersUnconstrained()
    {
        var result = Validator.Validate(Roster(50, 0, 1), skaterLimit: null, goalieLimit: 2);
        result.IsValid.Should().BeTrue();
    }
}
