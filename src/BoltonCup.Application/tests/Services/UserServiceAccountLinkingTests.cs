using BoltonCup.Application.Services;
using BoltonCup.Application.Tests.Helpers;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Persistence.Data;
using BoltonCup.Persistence.Identity;
using BoltonCup.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

// Postgres, not InMemory: linking is a conditional ExecuteUpdate and creation is INSERT ... ON CONFLICT.
[Collection(PostgresCollection.Name)]
public class UserServiceAccountLinkingTests(PostgresFixture postgres) : IAsyncLifetime
{
    const string Email = "player@example.com";

    IdentityTestHost _host = null!;

    public async Task InitializeAsync() => _host = new IdentityTestHost(postgresConnectionString: await postgres.CreateDatabaseAsync());

    public Task DisposeAsync()
    {
        _host.Dispose();
        return Task.CompletedTask;
    }

    async Task<Account> AddAccountAsync(string email, string? userId = null)
    {
        var account = new Account
        {
            FirstName = "Test",
            LastName = "Player",
            Email = email,
            Birthday = new DateTime(1990, 1, 1),
            UserId = userId,
        };
        _host.Db.Accounts.Add(account);
        await _host.Db.SaveChangesAsync();
        return account;
    }

    // Reads from a separate scope, so the result is what the database holds rather than a tracked entity.
    async Task<int?> OwnedAccountIdAsync(string userId)
    {
        using var scope = _host.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BoltonCupDbContext>();
        return await db.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => (int?)a.Id)
            .SingleOrDefaultAsync();
    }

    async Task<string> RegisterAndConfirmAsync(string email)
    {
        await _host.UserService.RegisterAsync(email, IdentityTestHost.Password);
        await _host.UserService.ConfirmEmailAsync(email, _host.SentCodes[email]);
        var user = await _host.UserManager.FindByEmailAsync(email);
        return user!.Id;
    }

    static CreateAccountCommand NewAccountCommand() => new(
        FirstName: "New",
        LastName: "Player",
        Email: Email,
        Birthday: new DateTime(1990, 1, 1),
        HeightFeet: 6,
        HeightInches: 0,
        Weight: 180,
        HighestLevel: null,
        PreferredBeer: null);

    [Fact]
    public async Task ConfirmEmail_LinksAccountWithMatchingEmailIgnoringCase()
    {
        var account = await AddAccountAsync("Player@Example.com");

        var userId = await RegisterAndConfirmAsync(Email);

        (await OwnedAccountIdAsync(userId)).Should().Be(account.Id);
    }

    [Fact]
    public async Task ConfirmEmail_SkipsAccountOwnedByAnotherUser()
    {
        var owner = await _host.CreateUserAsync("owner@example.com");
        await AddAccountAsync(Email, owner.Id);

        var userId = await RegisterAndConfirmAsync(Email);

        (await OwnedAccountIdAsync(userId)).Should().BeNull();
    }

    [Fact]
    public async Task DeletingUser_FreesTheirAccountForTheNextUserWithThatEmail()
    {
        var formerOwner = await _host.CreateUserAsync("former@example.com");
        var account = await AddAccountAsync(Email, formerOwner.Id);
        await _host.UserManager.DeleteAsync(formerOwner);

        var userId = await RegisterAndConfirmAsync(Email);

        (await OwnedAccountIdAsync(userId)).Should().Be(account.Id);
        (await OwnedAccountIdAsync(formerOwner.Id)).Should().BeNull();
    }

    [Fact]
    public async Task AccountUserId_RejectsAnIdWithNoUser()
    {
        var save = () => AddAccountAsync(Email, "no-such-user");

        await save.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ConfirmEmail_KeepsExistingAccountLink()
    {
        await _host.UserService.RegisterAsync(Email, IdentityTestHost.Password);
        var user = await _host.UserManager.FindByEmailAsync(Email);
        var ownAccount = await AddAccountAsync("own@example.com", user!.Id);
        var matchingAccount = await AddAccountAsync(Email);

        await _host.UserService.ConfirmEmailAsync(Email, _host.SentCodes[Email]);

        (await OwnedAccountIdAsync(user.Id)).Should().Be(ownAccount.Id);
        (await _host.Db.Accounts.AsNoTracking().SingleAsync(a => a.Id == matchingAccount.Id)).UserId.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_ConfirmsAndLinksUnconfirmedUser()
    {
        var account = await AddAccountAsync(Email);
        await _host.UserService.RegisterAsync(Email, IdentityTestHost.Password);
        await _host.UserService.ForgotPasswordAsync(Email);
        var code = _host.SentCodes[Email];

        await _host.UserService.VerifyPasswordResetCodeAsync(Email, code);
        await _host.UserService.ResetPasswordAsync(Email, code, IdentityTestHost.Password + "2");

        var user = await _host.UserManager.FindByEmailAsync(Email);
        user!.EmailConfirmed.Should().BeTrue();
        (await OwnedAccountIdAsync(user.Id)).Should().Be(account.Id);
    }

    [Fact]
    public async Task ResetPassword_LinksConfirmedUserWithoutAnAccount()
    {
        var account = await AddAccountAsync(Email);
        var user = await _host.CreateUserAsync(Email);
        await _host.UserService.ForgotPasswordAsync(Email);

        await _host.UserService.ResetPasswordAsync(Email, _host.SentCodes[Email], IdentityTestHost.Password + "2");

        (await OwnedAccountIdAsync(user.Id)).Should().Be(account.Id);
    }

    [Fact]
    public async Task LinkExistingAccount_TwoUsersInParallel_OnlyOneLinksTheAccount()
    {
        // Not reachable through registration today (usernames are emails); change-email will make it so.
        // Repeated, because one round may not overlap the two claims.
        for (var round = 0; round < 10; round++)
        {
            var email = $"race{round}@example.com";
            var account = await AddAccountAsync(email);
            var first = await _host.CreateUserAsync(email, userName: $"first{round}");
            var second = await _host.CreateUserAsync(email, userName: $"second{round}");

            var linked = await Task.WhenAll(LinkInOwnScopeAsync(first.Id), LinkInOwnScopeAsync(second.Id));

            linked.Should().ContainSingle(wasLinked => wasLinked, "only one claim may win round {0}", round);
            var winner = linked[0] ? first : second;
            (await OwnedAccountIdAsync(winner.Id)).Should().Be(account.Id);
        }
    }

    async Task<bool> LinkInOwnScopeAsync(string userId)
    {
        using var scope = _host.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BoltonCupUser>>();
        var userService = (UserService)scope.ServiceProvider.GetRequiredService<IUserService>();
        return await userService.LinkExistingAccountAsync((await userManager.FindByIdAsync(userId))!);
    }

    [Fact]
    public async Task CompleteUserAccount_DoubleSubmit_CreatesOneAccount()
    {
        var user = await _host.CreateUserAsync(Email);

        var accountIds = await Task.WhenAll(
            Enumerable.Range(0, 5).Select(_ => CompleteInOwnScopeAsync(user.Id)));

        accountIds.Distinct().Should().ContainSingle();
        (await _host.Db.Accounts.CountAsync(a => a.UserId == user.Id)).Should().Be(1);
        (await OwnedAccountIdAsync(user.Id)).Should().Be(accountIds[0]);
    }

    async Task<int> CompleteInOwnScopeAsync(string userId)
    {
        using var scope = _host.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await userService.CompleteUserAccountAsync(userId, NewAccountCommand());
        var db = scope.ServiceProvider.GetRequiredService<BoltonCupDbContext>();
        return await db.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .SingleAsync();
    }

    [Fact]
    public async Task CompleteUserAccount_UserAlreadyOwnsAccount_KeepsIt()
    {
        var user = await _host.CreateUserAsync(Email);
        var existing = await AddAccountAsync(Email, user.Id);

        await _host.UserService.CompleteUserAccountAsync(user.Id, NewAccountCommand());

        (await OwnedAccountIdAsync(user.Id)).Should().Be(existing.Id);
        (await _host.Db.Accounts.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CompleteUserAccount_UnownedAccountWithTheirEmail_ClaimsItInsteadOfCreating()
    {
        var existing = await AddAccountAsync("Player@Example.com");
        var user = await _host.CreateUserAsync(Email);

        await _host.UserService.CompleteUserAccountAsync(user.Id, NewAccountCommand());

        (await OwnedAccountIdAsync(user.Id)).Should().Be(existing.Id);
        (await _host.Db.Accounts.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SignIn_ClaimsCarryTheOwnedAccountId()
    {
        var user = await _host.CreateUserAsync(Email);
        var account = await AddAccountAsync(Email, user.Id);
        var claimsFactory = _host.Services.GetRequiredService<IUserClaimsPrincipalFactory<BoltonCupUser>>();

        var principal = await claimsFactory.CreateAsync(user);

        principal.FindFirst(BoltonCupClaimTypes.AccountId)!.Value.Should().Be(account.Id.ToString());
    }
}