using BoltonCup.Application.Services;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class AccountServiceTests
{
    const int AccountId = 1;

    static BoltonCupDbContext NewContext() => new BoltonCupDbContext(new DbContextOptionsBuilder<BoltonCupDbContext>().UseInMemoryDatabase($"account-{Guid.NewGuid()}").Options);

    static async Task<BoltonCupDbContext> SeededContextAsync()
    {
        var db = NewContext();
        db.Accounts.Add(new Account
        {
            Id = AccountId,
            FirstName = "Test",
            LastName = "Player",
            Email = "player@example.com",
            Birthday = new DateTime(1995, 1, 1),
            Hometown = "Toronto, ON",
        });
        await db.SaveChangesAsync();
        return db;
    }

    static AccountService NewService(BoltonCupDbContext db) => new AccountService(db, Mock.Of<IStorageService>(), Mock.Of<IAssetKeyGenerator>());

    static UpdateAccountCommand Command(string? hometown) => new UpdateAccountCommand(AccountId: AccountId, FirstName: "Test", LastName: "Player", Birthday: new DateTime(1995, 1, 1), HighestLevel: null, PreferredBeer: null, HeightFeet: null, HeightInches: null, Weight: null, Hometown: hometown);

    [Fact]
    public async Task UpdateAsync_StoresTrimmedHometown()
    {
        await using var db = await SeededContextAsync();

        await NewService(db).UpdateAsync(Command("  Windsor, ON  "));

        var account = await db.Accounts.SingleAsync(a => a.Id == AccountId);
        account.Hometown.Should().Be("Windsor, ON");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_StoresNullWhenHometownIsBlank(string? hometown)
    {
        await using var db = await SeededContextAsync();

        await NewService(db).UpdateAsync(Command(hometown));

        var account = await db.Accounts.SingleAsync(a => a.Id == AccountId);
        account.Hometown.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Windsor, ON")]
    [InlineData("Windsor")]
    public void Validator_AcceptsAnyHometownFormat(string? hometown)
    {
        var result = new UpdateAccountCommandValidator().Validate(Command(hometown));

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateAccountCommand.Hometown));
    }

    [Fact]
    public void Validator_RejectsHometownOverFiftyCharacters()
    {
        var result = new UpdateAccountCommandValidator().Validate(Command(new string('a', 51)));

        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountCommand.Hometown));
    }
}