using BoltonCup.Application.Tests.Helpers;
using BoltonCup.Core;
using FluentAssertions;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class UserServiceAccountLinkingTests : IDisposable
{
    readonly IdentityTestHost _host = new();

    public void Dispose() => _host.Dispose();

    async Task<Account> AddAccountAsync(string email)
    {
        var account = new Account
        {
            FirstName = "Test",
            LastName = "Player",
            Email = email,
            Birthday = new DateTime(1990, 1, 1),
        };
        _host.Db.Accounts.Add(account);
        await _host.Db.SaveChangesAsync();
        return account;
    }

    async Task<int?> RegisterAndConfirmAsync(string email)
    {
        await _host.UserService.RegisterAsync(email, IdentityTestHost.Password);
        await _host.UserService.ConfirmEmailAsync(email, _host.SentCodes[email]);
        var user = await _host.UserManager.FindByEmailAsync(email);
        return user!.AccountId;
    }

    [Fact]
    public async Task ConfirmEmail_LinksAccountWithMatchingEmailIgnoringCase()
    {
        var account = await AddAccountAsync("Player@Example.com");

        var accountId = await RegisterAndConfirmAsync("player@example.com");

        accountId.Should().Be(account.Id);
    }

    [Fact]
    public async Task ConfirmEmail_SkipsAccountOwnedByAnotherUser()
    {
        var account = await AddAccountAsync("player@example.com");
        await _host.CreateUserAsync("owner@example.com", accountId: account.Id);

        var accountId = await RegisterAndConfirmAsync("player@example.com");

        accountId.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmEmail_KeepsExistingAccountLink()
    {
        var ownAccount = await AddAccountAsync("own@example.com");
        await AddAccountAsync("player@example.com");
        await _host.UserService.RegisterAsync("player@example.com", IdentityTestHost.Password);
        var user = await _host.UserManager.FindByEmailAsync("player@example.com");
        user!.AccountId = ownAccount.Id;
        await _host.UserManager.UpdateAsync(user);

        await _host.UserService.ConfirmEmailAsync("player@example.com", _host.SentCodes["player@example.com"]);

        var confirmed = await _host.UserManager.FindByEmailAsync("player@example.com");
        confirmed!.AccountId.Should().Be(ownAccount.Id);
    }

    [Fact]
    public async Task ResetPassword_ConfirmsAndLinksUnconfirmedUser()
    {
        var account = await AddAccountAsync("player@example.com");
        await _host.UserService.RegisterAsync("player@example.com", IdentityTestHost.Password);
        await _host.UserService.ForgotPasswordAsync("player@example.com");
        var code = _host.SentCodes["player@example.com"];

        await _host.UserService.VerifyPasswordResetCodeAsync("player@example.com", code);
        await _host.UserService.ResetPasswordAsync("player@example.com", code, IdentityTestHost.Password + "2");

        var user = await _host.UserManager.FindByEmailAsync("player@example.com");
        user!.EmailConfirmed.Should().BeTrue();
        user.AccountId.Should().Be(account.Id);
    }
}