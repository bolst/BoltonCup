using BoltonCup.Application.Exceptions;
using BoltonCup.Application.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class UserServiceLoginLockoutTests : IDisposable
{
    const string Email = "player@example.com";
    const string WrongPassword = "Wrong-Password-1";

    readonly IdentityTestHost _host = new();

    int MaxFailedAttempts => _host.UserManager.Options.Lockout.MaxFailedAccessAttempts;

    public void Dispose() => _host.Dispose();

    Task LoginAsync(string password) => _host.UserService.LoginAsync(Email, password, persist: false);

    async Task FailLoginAsync(int times)
    {
        for (var i = 0; i < times; i++)
        {
            var login = () => LoginAsync(WrongPassword);
            await login.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }

    async Task LockOutAsync()
    {
        await FailLoginAsync(MaxFailedAttempts - 1);
        var tenth = () => LoginAsync(WrongPassword);
        await tenth.Should().ThrowAsync<AccountLockedOutException>();
    }

    [Fact]
    public async Task Login_TenthWrongPassword_LocksTheAccount()
    {
        var user = await _host.CreateUserAsync(Email);

        await LockOutAsync();

        user = (await _host.UserManager.FindByIdAsync(user.Id))!;
        user.LockoutEnd.Should().NotBeNull();
        _host.Telemetry.Verify(t => t.TrackEvent("auth.login_locked_out", "user.id", user.Id), Times.Once);
    }

    [Fact]
    public async Task Login_WhenLockedOut_RejectsTheCorrectPassword()
    {
        await _host.CreateUserAsync(Email);
        await LockOutAsync();

        var login = () => LoginAsync(IdentityTestHost.Password);

        await login.Should().ThrowAsync<AccountLockedOutException>();
    }

    [Fact]
    public async Task Login_CorrectPasswordBeforeTheLimit_ResetsTheFailureCount()
    {
        var user = await _host.CreateUserAsync(Email);
        await FailLoginAsync(MaxFailedAttempts - 1);

        await LoginAsync(IdentityTestHost.Password);

        user = (await _host.UserManager.FindByIdAsync(user.Id))!;
        user.AccessFailedCount.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsInvalidCredentials()
    {
        var login = () => LoginAsync(IdentityTestHost.Password);

        await login.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task ResetPassword_WhenLockedOut_ClearsTheLockout()
    {
        var user = await _host.CreateUserAsync(Email);
        await LockOutAsync();
        await _host.UserService.ForgotPasswordAsync(Email);

        await _host.UserService.ResetPasswordAsync(Email, _host.SentCodes[Email], "New-Password-2");

        var login = () => LoginAsync("New-Password-2");
        await login.Should().NotThrowAsync();
        _host.Telemetry.Verify(t => t.TrackEvent("auth.lockout_cleared_by_reset", "user.id", user.Id), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenNotLockedOut_DoesNotRecordAClear()
    {
        await _host.CreateUserAsync(Email);
        await _host.UserService.ForgotPasswordAsync(Email);

        await _host.UserService.ResetPasswordAsync(Email, _host.SentCodes[Email], "New-Password-2");

        _host.Telemetry.Verify(t => t.TrackEvent("auth.lockout_cleared_by_reset", It.IsAny<object?[]>()), Times.Never);
    }
}