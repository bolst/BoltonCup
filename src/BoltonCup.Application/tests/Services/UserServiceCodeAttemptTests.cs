using BoltonCup.Application.Exceptions;
using BoltonCup.Application.Services;
using BoltonCup.Application.Tests.Helpers;
using BoltonCup.Persistence.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class UserServiceCodeAttemptTests : IDisposable
{
    const string Email = "player@example.com";

    readonly IdentityTestHost _host = new();

    public void Dispose() => _host.Dispose();

    static string WrongCode(string code) => ((int.Parse(code) + 1) % 1_000_000).ToString("D6");

    async Task<string> RequestResetCodeAsync()
    {
        await _host.UserService.ForgotPasswordAsync(Email);
        return _host.SentCodes[Email];
    }

    async Task<string> RegisterAsync()
    {
        await _host.UserService.RegisterAsync(Email, IdentityTestHost.Password);
        return _host.SentCodes[Email];
    }

    async Task ExhaustResetAttemptsAsync(string code)
    {
        for (var i = 0; i < CodeAttemptGuard.MaxAttempts; i++)
        {
            var act = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, WrongCode(code));
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }

    async Task ExhaustConfirmationAttemptsAsync(string code)
    {
        for (var i = 0; i < CodeAttemptGuard.MaxAttempts; i++)
        {
            var act = () => _host.UserService.ConfirmEmailAsync(Email, WrongCode(code));
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }

    [Fact]
    public async Task PasswordReset_AfterTenWrongCodes_RejectsEvenTheCorrectCode()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();

        await ExhaustResetAttemptsAsync(code);

        var verify = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, code);
        await verify.Should().ThrowAsync<TooManyAttemptsException>();
        var reset = () => _host.UserService.ResetPasswordAsync(Email, code, "New-Password-2");
        await reset.Should().ThrowAsync<TooManyAttemptsException>();
    }

    [Fact]
    public async Task PasswordReset_WrongCodes_DoNotTouchIdentityLockout()
    {
        var user = await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();

        await ExhaustResetAttemptsAsync(code);
        var verify = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, code);
        await verify.Should().ThrowAsync<TooManyAttemptsException>();

        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
        var login = () => _host.UserService.LoginAsync(Email, IdentityTestHost.Password, persist: false);
        await login.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PasswordReset_ResendingTheCode_DoesNotResetTheWindow()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();
        await ExhaustResetAttemptsAsync(code);

        var newCode = await RequestResetCodeAsync();

        var verify = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, newCode);
        await verify.Should().ThrowAsync<TooManyAttemptsException>();
    }

    [Fact]
    public async Task PasswordReset_WindowResetsAfterOneHour()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();
        await ExhaustResetAttemptsAsync(code);

        _host.Clock.Advance(CodeAttemptGuard.Window);
        var newCode = await RequestResetCodeAsync();

        var verify = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, newCode);
        await verify.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PasswordReset_CorrectCode_ClearsTheWindow()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();
        for (var i = 0; i < CodeAttemptGuard.MaxAttempts - 1; i++)
        {
            var wrong = () => _host.UserService.VerifyPasswordResetCodeAsync(Email, WrongCode(code));
            await wrong.Should().ThrowAsync<InvalidCredentialsException>();
        }

        await _host.UserService.VerifyPasswordResetCodeAsync(Email, code);

        await ExhaustResetAttemptsAsync(code);
    }

    [Fact]
    public async Task ResetPassword_WrongCode_ThrowsInvalidCredentials()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();

        var reset = () => _host.UserService.ResetPasswordAsync(Email, WrongCode(code), "New-Password-2");

        await reset.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task ResetPassword_CorrectCode_ChangesThePassword()
    {
        await _host.CreateUserAsync(Email);
        var code = await RequestResetCodeAsync();

        await _host.UserService.ResetPasswordAsync(Email, code, "New-Password-2");

        var login = () => _host.UserService.LoginAsync(Email, "New-Password-2", persist: false);
        await login.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EmailConfirmation_AfterTenWrongCodes_RejectsEvenTheCorrectCode()
    {
        var code = await RegisterAsync();

        await ExhaustConfirmationAttemptsAsync(code);

        var confirm = () => _host.UserService.ConfirmEmailAsync(Email, code);
        await confirm.Should().ThrowAsync<TooManyAttemptsException>();
        var user = await _host.UserManager.FindByEmailAsync(Email);
        user!.EmailConfirmed.Should().BeFalse();
        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public async Task EmailConfirmation_ResendingTheCode_DoesNotResetTheWindow()
    {
        var code = await RegisterAsync();
        await ExhaustConfirmationAttemptsAsync(code);

        await _host.UserService.ResendConfirmationEmailAsync(Email);
        var newCode = _host.SentCodes[Email];

        var confirm = () => _host.UserService.ConfirmEmailAsync(Email, newCode);
        await confirm.Should().ThrowAsync<TooManyAttemptsException>();
    }

    [Fact]
    public async Task EmailConfirmation_WindowResetsAfterOneHour()
    {
        var code = await RegisterAsync();
        await ExhaustConfirmationAttemptsAsync(code);

        _host.Clock.Advance(CodeAttemptGuard.Window);
        await _host.UserService.ResendConfirmationEmailAsync(Email);
        await _host.UserService.ConfirmEmailAsync(Email, _host.SentCodes[Email]);

        var user = await _host.UserManager.FindByEmailAsync(Email);
        user!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task CodeCheck_WhenAttemptCannotBeSaved_FailsClosed()
    {
        using var host = new IdentityTestHost(services =>
            services.AddScoped<UserManager<BoltonCupUser>, TokenSaveFailingUserManager>());
        await host.CreateUserAsync(Email);
        await host.UserService.ForgotPasswordAsync(Email);

        var verify = () => host.UserService.VerifyPasswordResetCodeAsync(Email, host.SentCodes[Email]);

        await verify.Should().ThrowAsync<AttemptNotRecordedException>();
    }

    [Fact]
    public async Task Login_LockedOutUser_ThrowsAccountLockedOut()
    {
        var user = await _host.CreateUserAsync(Email);
        await _host.UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(1));

        var login = () => _host.UserService.LoginAsync(Email, IdentityTestHost.Password, persist: false);

        await login.Should().ThrowAsync<AccountLockedOutException>();
    }

    // Simulates a save that loses a race (e.g. ConcurrencyFailure from parallel guesses).
    sealed class TokenSaveFailingUserManager(
        IUserStore<BoltonCupUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<BoltonCupUser> passwordHasher,
        IEnumerable<IUserValidator<BoltonCupUser>> userValidators,
        IEnumerable<IPasswordValidator<BoltonCupUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<BoltonCupUser>> logger)
        : UserManager<BoltonCupUser>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        public override Task<IdentityResult> SetAuthenticationTokenAsync(BoltonCupUser user, string loginProvider, string tokenName, string? tokenValue) =>
            Task.FromResult(IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure()));
    }
}