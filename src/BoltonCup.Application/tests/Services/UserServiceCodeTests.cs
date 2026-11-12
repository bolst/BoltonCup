using BoltonCup.Application.Exceptions;
using BoltonCup.Application.Tests.Helpers;
using BoltonCup.Persistence.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

// Attempt caps live in the API (AuthAttemptLimiter); these cover what a right or wrong code does.
public class UserServiceCodeTests : IDisposable
{
    const string Email = "player@example.com";
    const string NewPassword = "New-Password-2";

    readonly IdentityTestHost _host = new();

    public void Dispose() => _host.Dispose();

    static string WrongCode(string code) => ((int.Parse(code) + 1) % 1_000_000).ToString("D6");

    async Task<string> RequestResetCodeAsync()
    {
        await _host.UserService.ForgotPasswordAsync(Email);
        return _host.SentCodes[Email];
    }

    async Task<bool> IsEmailConfirmedAsync() => (await _host.UserManager.FindByEmailAsync(Email))!.EmailConfirmed;

    [Fact]
    public async Task VerifyPasswordResetCode_WrongCode_ThrowsInvalidCredentials()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();

        var verify = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, WrongCode(code));

        await verify.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task VerifyPasswordResetCode_ManyWrongCodes_StillAcceptsTheRightOne()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();
        for (var i = 0; i < 20; i++)
        {
            var wrong = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, WrongCode(code));
            await wrong.Should().ThrowAsync<InvalidCredentialsException>();
        }

        var verify = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, code);

        await verify.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ResetPassword_WrongCode_ThrowsInvalidCredentialsAndLeavesEmailUnconfirmed()
    {
        await _host.CreateUserAsync(Email, emailConfirmed: false);
        var code = await RequestResetCodeAsync();

        var reset = () => _host.UserService.ResetPasswordAsync(Email, WrongCode(code), NewPassword);

        await reset.Should().ThrowAsync<InvalidCredentialsException>();
        (await IsEmailConfirmedAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task ResetPassword_CorrectCode_ChangesThePasswordAndConfirmsTheEmail()
    {
        await _host.CreateUserAsync(Email, emailConfirmed: false);
        var code = await RequestResetCodeAsync();

        await _host.UserService.ResetPasswordAsync(Email, code, NewPassword);

        (await IsEmailConfirmedAsync()).Should().BeTrue();
        var login = () => _host.UserService.LoginAsync(Email, NewPassword, persist: false);
        await login.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ResetPassword_RejectedNewPassword_LeavesEmailUnconfirmed()
    {
        await _host.CreateUserAsync(Email, emailConfirmed: false);
        var code = await RequestResetCodeAsync();

        var reset = () => _host.UserService.ResetPasswordAsync(Email, code, "short");

        await reset.Should().ThrowAsync<PasswordResetFailedException>();
        using var scope = _host.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BoltonCupUser>>();
        (await userManager.FindByEmailAsync(Email))!.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmEmail_WrongCode_ThrowsInvalidCredentials()
    {
        await _host.UserService.RegisterAsync(Email, IdentityTestHost.Password);
        var code = _host.SentCodes[Email];

        var confirm = () => _host.UserService.ConfirmEmailAsync(Email, WrongCode(code));

        await confirm.Should().ThrowAsync<InvalidCredentialsException>();
        (await IsEmailConfirmedAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmEmail_CorrectCode_ConfirmsTheEmail()
    {
        await _host.UserService.RegisterAsync(Email, IdentityTestHost.Password);

        await _host.UserService.ConfirmEmailAsync(Email, _host.SentCodes[Email]);

        (await IsEmailConfirmedAsync()).Should().BeTrue();
    }
}