using BoltonCup.Core;
using FluentAssertions;
using Xunit;

namespace BoltonCup.Application.Tests.Entities;

public class EntityComparerTests
{
    static Franchise NewFranchise(int id) => new()
    {
        Id = id,
        Name = "Franchise",
        NameShort = "F",
        Abbreviation = "FR",
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#FFFFFF",
    };

    static Team NewTeam(int id) => new()
    {
        Id = id,
        Name = "Team",
        NameShort = "T",
        Abbreviation = "TM",
        PrimaryColorHex = "#000000",
        SecondaryColorHex = "#FFFFFF",
    };

    [Fact]
    public void FranchiseComparer_UnsavedRows_StayDistinct()
    {
        var set = new HashSet<Franchise>(new FranchiseComparer()) { NewFranchise(0), NewFranchise(0) };

        set.Should().HaveCount(2);
    }

    [Fact]
    public void FranchiseComparer_SavedRowsWithSameId_AreEqual()
    {
        var comparer = new FranchiseComparer();

        comparer.Equals(NewFranchise(3), NewFranchise(3)).Should().BeTrue();
        comparer.GetHashCode(NewFranchise(3)).Should().Be(comparer.GetHashCode(NewFranchise(3)));
    }

    [Fact]
    public void FranchiseComparer_SameUnsavedInstance_IsEqualToItself()
    {
        var comparer = new FranchiseComparer();
        var franchise = NewFranchise(0);

        comparer.Equals(franchise, franchise).Should().BeTrue();
        new HashSet<Franchise>(comparer) { franchise }.Contains(franchise).Should().BeTrue();
    }

    [Fact]
    public void TeamComparer_UnsavedRows_StayDistinct()
    {
        var set = new HashSet<Team>(new TeamComparer()) { NewTeam(0), NewTeam(0) };

        set.Should().HaveCount(2);
    }

    [Fact]
    public void TeamComparer_SavedRowsWithSameId_AreEqual()
    {
        var comparer = new TeamComparer();

        comparer.Equals(NewTeam(7), NewTeam(7)).Should().BeTrue();
        comparer.GetHashCode(NewTeam(7)).Should().Be(comparer.GetHashCode(NewTeam(7)));
    }
}