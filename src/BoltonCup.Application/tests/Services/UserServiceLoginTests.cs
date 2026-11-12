using BoltonCup.Application.Exceptions;
using BoltonCup.Application.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

// Attempt caps live in the API (AuthAttemptLimiter), so wrong passwords here never lock anyone out.
public class UserServiceLoginTests : IDisposable
{
    const string Email = "player@example.com";
    const string WrongPassword = "Wrong-Password-1";

    readonly IdentityTestHost _host = new();

    public void Dispose() => _host.Dispose();

    Task LoginAsync(string password) => _host.UserService.LoginAsync(Email, password, persist: false);

    [Fact]
    public async Task Login_ManyWrongPasswords_DoNotLockTheUser()
    {
        var user = await _host.CreateUserAsync(Email);
        for (var i = 0; i < 20; i++)
        {
            var wrong = () => LoginAsync(WrongPassword);
            await wrong.Should().ThrowAsync<InvalidCredentialsException>();
        }

        var login = () => LoginAsync(IdentityTestHost.Password);

        await login.Should().NotThrowAsync();
        user = (await _host.UserManager.FindByIdAsync(user.Id))!;
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task Login_UserLockedBeforeThisChange_ThrowsAccountLockedOut()
    {
        var user = await _host.CreateUserAsync(Email);
        await _host.UserManager.SetLockoutEnabledAsync(user, true);
        await _host.UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));

        var login = () => LoginAsync(IdentityTestHost.Password);

        await login.Should().ThrowAsync<AccountLockedOutException>();
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsInvalidCredentials()
    {
        var login = () => LoginAsync(IdentityTestHost.Password);

        await login.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Login_UnconfirmedEmail_ThrowsAccountNotConfirmed()
    {
        await _host.CreateUserAsync(Email, emailConfirmed: false);

        var login = () => LoginAsync(IdentityTestHost.Password);

        await login.Should().ThrowAsync<AccountNotConfirmedException>();
    }
}