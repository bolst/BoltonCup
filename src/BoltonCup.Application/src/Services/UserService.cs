using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Exceptions;
using BoltonCup.Application.Extensions;
using BoltonCup.Integrations.Email;
using BoltonCup.Persistence.Identity;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Application.Services;

public interface IUserService
{
    Task<Account?> GetMyAccountAsync(ClaimsPrincipal claimsPrincipal);
    Task LoginAsync(string email, string password, bool persist);
    Task RegisterAsync(string email, string password);
    Task ResendConfirmationEmailAsync(string email);
    Task VerifyPasswordResetCodeAsync(string email, string code);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string email, string code, string newPassword);
    Task ConfirmEmailAsync(string email, string code);
    Task<BoltonCupUser> CompleteUserAccountAsync(string userId, CreateAccountCommand command);
}

class UserService(
    BoltonCupDbContext _dbContext,
    UserManager<BoltonCupUser> _userManager,
    SignInManager<BoltonCupUser> _signInManager,
    IAccountService _accountService,
    IEmailer _emailer,
    CodeAttemptGuard _codeAttemptGuard,
    ITelemetry _telemetry,
    ILogger<UserService> _logger) : IUserService
{

    public async Task<Account?> GetMyAccountAsync(ClaimsPrincipal claimsPrincipal)
    {
        if (!claimsPrincipal.TryGetAccountId(out var accountId))
        {
            return null;
        }

        return await _dbContext.Accounts.FindAsync(accountId);
    }


    public async Task LoginAsync(string email, string password, bool persist)
    {
        if (await _userManager.FindByNameAsync(email) is not { } user)
        {
            throw new InvalidCredentialsException();
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, persist, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login blocked by lockout for user {UserId}", user.Id);
            _telemetry.TrackEvent("auth.login_locked_out", "user.id", user.Id);
            throw new AccountLockedOutException();
        }

        if (result == SignInResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        if (result == SignInResult.NotAllowed)
        {
            throw new AccountNotConfirmedException();
        }
    }


    public async Task RegisterAsync(string email, string password)
    {
        var user = new BoltonCupUser();
        await _userManager.SetUserNameAsync(user, email);
        await _userManager.SetEmailAsync(user, email);

        if (await _userManager.CreateAsync(user, password) is { Succeeded: false } failResult)
        {
            throw new UserRegistrationFailedException(failResult);
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailer.SendConfirmationCodeAsync(email, code);
    }



    public async Task ResendConfirmationEmailAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
        {
            return;
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailer.SendConfirmationCodeAsync(email, code);
    }



    public async Task VerifyPasswordResetCodeAsync(string email, string code)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
        {
            throw new InvalidCredentialsException();
        }

        await CheckPasswordResetCodeAsync(user, code);
    }



    public async Task ForgotPasswordAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
        {
            return;
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailer.SendPasswordResetCodeAsync(email, code);
    }



    public async Task ResetPasswordAsync(string email, string code, string newPassword)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
        {
            throw new InvalidCredentialsException();
        }

        await CheckPasswordResetCodeAsync(user, code);

        if (await _userManager.ResetPasswordAsync(user, code, newPassword) is { Succeeded: false } result)
        {
            _logger.LogWarning(
                "Password reset failed for user {UserId}: {Errors}",
                user.Id,
                string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
            throw new PasswordResetFailedException(result);
        }

        // The reset code proves control of the email: lift any login lockout a stranger caused and confirm the account.
        var wasLockedOut = await _userManager.IsLockedOutAsync(user);
        var wasUnconfirmed = !user.EmailConfirmed;
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        user.EmailConfirmed = true;
        if (await _userManager.UpdateAsync(user) is { Succeeded: false } updateResult)
        {
            _logger.LogWarning(
                "Could not clear lockout or confirm email after reset for user {UserId}: {ErrorCodes}",
                user.Id,
                string.Join(", ", updateResult.Errors.Select(e => e.Code)));
            return;
        }

        if (wasLockedOut)
        {
            _telemetry.TrackEvent("auth.lockout_cleared_by_reset", "user.id", user.Id);
        }

        if (wasUnconfirmed)
        {
            await LinkExistingAccountAsync(user);
        }
    }



    public async Task ConfirmEmailAsync(string email, string code)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
        {
            throw new InvalidCredentialsException();
        }

        await _codeAttemptGuard.ConsumeAttemptAsync(user, CodePurpose.EmailConfirmation);
        var isValidCode = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.EmailConfirmationTokenProvider,
            UserManager<BoltonCupUser>.ConfirmEmailTokenPurpose,
            code);
        if (!isValidCode)
        {
            throw new InvalidCredentialsException();
        }

        await _codeAttemptGuard.ClearAsync(user, CodePurpose.EmailConfirmation);

        if (await _userManager.ConfirmEmailAsync(user, code) is { Succeeded: false })
        {
            throw new InvalidCredentialsException();
        }

        await LinkExistingAccountAsync(user);
    }



    public async Task<BoltonCupUser> CompleteUserAccountAsync(string userId, CreateAccountCommand command)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new EntityNotFoundException("User", userId);
        if (user.AccountId.HasValue)
        {
            return user;
        }

        var accountId = await _accountService.CreateAsync(command);
        user.AccountId = accountId;
        await _userManager.UpdateAsync(user);

        return user;
    }

    // Joins an existing account by email, unless the user already has one or another user owns it.
    async Task LinkExistingAccountAsync(BoltonCupUser user)
    {
        if (user.AccountId is not null || user.Email is null)
        {
            return;
        }

        var email = user.Email.ToLowerInvariant();
        if (await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email.ToLower() == email) is not { } account)
        {
            return;
        }

        if (await _userManager.Users.AnyAsync(u => u.AccountId == account.Id))
        {
            return;
        }

        user.AccountId = account.Id;
        await _userManager.UpdateAsync(user);
    }

    // Consumes an attempt before checking so parallel or repeated guesses cannot exceed the per-user cap.
    async Task CheckPasswordResetCodeAsync(BoltonCupUser user, string code)
    {
        await _codeAttemptGuard.ConsumeAttemptAsync(user, CodePurpose.PasswordReset);
        var isValidCode = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<BoltonCupUser>.ResetPasswordTokenPurpose,
            code);
        if (!isValidCode)
        {
            throw new InvalidCredentialsException();
        }

        await _codeAttemptGuard.ClearAsync(user, CodePurpose.PasswordReset);
    }
}