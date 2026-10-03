using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace BoltonCup.Application.Tests.Persistence;

// InMemory ignores FKs, so these check the relational model the migration is generated from.
public class FranchiseModelTests
{
    static readonly IModel Model = BuildModel();

    static IModel BuildModel()
    {
        // Building the model needs no connection.
        using var db = new BoltonCupDbContext(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseNpgsql("Host=localhost;Database=franchise-model-test")
            .Options);
        return db.GetService<IDesignTimeModel>().Model;
    }

    static IForeignKey TeamFranchiseForeignKey() => Model.FindEntityType(typeof(Team))!
        .GetForeignKeys()
        .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Franchise));

    [Fact]
    public void TeamFranchiseId_IsRequired()
    {
        var foreignKey = TeamFranchiseForeignKey();

        foreignKey.IsRequired.Should().BeTrue();
        foreignKey.Properties.Should().ContainSingle().Which.Name.Should().Be(nameof(Team.FranchiseId));
        foreignKey.Properties[0].IsNullable.Should().BeFalse();
        foreignKey.Properties[0].GetColumnName().Should().Be("franchise_id");
    }

    [Fact]
    public void TeamFranchiseForeignKey_RestrictsDelete()
    {
        TeamFranchiseForeignKey().DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    [Fact]
    public void FranchiseOwners_KeyIsFranchiseAndAccount()
    {
        var owners = Model.FindEntityType(typeof(FranchiseOwner))!;

        owners.GetTableName().Should().Be("franchise_owners");
        owners.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()).Should().Equal("franchise_id", "account_id");
        owners.GetForeignKeys().Should().HaveCount(2).And.OnlyContain(fk => fk.DeleteBehavior == DeleteBehavior.Cascade);
    }
}