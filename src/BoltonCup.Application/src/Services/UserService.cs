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
using Npgsql;

namespace BoltonCup.Application.Services;

public interface IUserService
{
    Task<Account?> GetMyAccountAsync(ClaimsPrincipal claimsPrincipal);
    Task LoginAsync(string email, string password, bool persist, bool lockoutOnFailure = false);
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


    public async Task LoginAsync(string email, string password, bool persist, bool lockoutOnFailure = false)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, persist, lockoutOnFailure);

        if (result.IsLockedOut)
        {
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

        // a user could only get here if they got the code via email
        // thus, confirm their account if needed
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
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
        await _accountService.CreateAsync(userId, command);

        return user;
    }

    // Joins the unowned account with the user's email, unless the user already owns an account. Returns whether it
    // linked. Internal so tests can race two users, which the public, email-keyed methods can't express.
    internal async Task<bool> LinkExistingAccountAsync(BoltonCupUser user)
    {
        if (user.Email is null)
        {
            return false;
        }

        var email = user.Email.ToLowerInvariant();
        var candidateId = await _dbContext.Accounts
            .Where(a => a.Email.ToLower() == email && a.UserId == null)
            .OrderBy(a => a.Id)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
        if (candidateId is null)
        {
            return false;
        }

        // Postgres re-checks user_id IS NULL under the row lock, so of two parallel claims only one updates the row.
        try
        {
            var linkedRows = await _dbContext.Accounts
                .Where(a => a.Id == candidateId
                    && a.UserId == null
                    && !_dbContext.Accounts.Any(owned => owned.UserId == user.Id))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.UserId, user.Id)
                    .SetProperty(a => a.LastModified, DateTime.UtcNow));

            return linkedRows == 1;
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // The ownership subquery reads the statement's snapshot, so an account the user created in parallel
            // surfaces here, at the unique index, instead.
            return false;
        }
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