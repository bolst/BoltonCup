using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace BoltonCup.Application.Tests.Persistence;

// The link races rely on this index, so a model change that drops it must fail here, not in prod.
public class AccountUserLinkModelTests
{
    static IModel BuildModel()
    {
        // Building the model needs no connection.
        using var db = new BoltonCupDbContext(new DbContextOptionsBuilder<BoltonCupDbContext>()
            .UseNpgsql("Host=localhost;Database=account-user-link-model-test")
            .Options);
        return db.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void AccountUserId_HasUniqueIndex()
    {
        var account = BuildModel().FindEntityType(typeof(Account))!;
        var userId = account.FindProperty(nameof(Account.UserId))!;

        userId.GetColumnName().Should().Be("user_id");
        userId.IsNullable.Should().BeTrue();
        account.GetIndexes().Should().ContainSingle(index => index.Properties.SequenceEqual(new[] { userId }))
            .Which.IsUnique.Should().BeTrue();
    }
}