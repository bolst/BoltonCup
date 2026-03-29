using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Exceptions;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

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

public class UserService(
    BoltonCupDbContext _dbContext,
    UserManager<BoltonCupUser> _userManager,
    SignInManager<BoltonCupUser> _signInManager,
    IAccountService _accountService,
    IEmailer _emailer) : IUserService
{

    public async Task<Account?> GetMyAccountAsync(ClaimsPrincipal claimsPrincipal)
    {
        if (!claimsPrincipal.TryGetAccountId(out var accountId))
            return null;
        return await _dbContext.Accounts.FindAsync(accountId);
    }


    public async Task LoginAsync(string email, string password, bool persist, bool lockoutOnFailure = false)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, persist, lockoutOnFailure);

        if (result == SignInResult.Failed)
            throw new InvalidCredentialsException();

        if (result == SignInResult.NotAllowed)
            throw new AccountNotConfirmedException();
    }
    
    
    public async Task RegisterAsync(string email, string password)
    {
        var user = new BoltonCupUser();
        await _userManager.SetUserNameAsync(user, email);
        await _userManager.SetEmailAsync(user, email);

        if (await _userManager.CreateAsync(user, password) is { Succeeded: false } failResult)
            throw new UserRegistrationFailedException(failResult);
        
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user); 
        await _emailer.SendConfirmationCodeAsync(user, email, code);
    }

    
    
    public async Task ResendConfirmationEmailAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return;
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailer.SendConfirmationCodeAsync(user, email, code);
    }
    
    
    
    public async Task VerifyPasswordResetCodeAsync(string email, string code)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            throw new InvalidCredentialsException();

        var isValidToken = await _userManager.VerifyUserTokenAsync(
                   user: user, 
                   tokenProvider: _userManager.Options.Tokens.PasswordResetTokenProvider,
                   purpose: UserManager<BoltonCupUser>.ResetPasswordTokenPurpose,
                   token: code
                );
        if (!isValidToken)
            throw new InvalidCredentialsException();
    }

    
    
    public async Task ForgotPasswordAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return;
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailer.SendPasswordResetCodeAsync(user, email, code);
    }

    
    
    public async Task ResetPasswordAsync(string email, string code, string newPassword)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            throw new InvalidCredentialsException();

        if (await _userManager.ResetPasswordAsync(user, code, newPassword) is { Succeeded: false })
            throw new InvalidCredentialException();

        // a user could only get here if they got the code via email
        // thus, confirm their account if needed
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }
    }



    public async Task ConfirmEmailAsync(string email, string code)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            throw new InvalidCredentialsException();

        if (await _userManager.ConfirmEmailAsync(user, code) is { Succeeded: false })
            throw new InvalidCredentialsException();
        
        // if a user already has an account, join it to them
        if (await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == user.Email) is { } account)
        {
            user.AccountId = account.Id;
            await _userManager.UpdateAsync(user);
        }
    }



    public async Task<BoltonCupUser> CompleteUserAccountAsync(string userId, CreateAccountCommand command)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new EntityNotFoundException("User", userId);
        
        var accountId = await _accountService.CreateAsync(command);
        user.AccountId = accountId;
        await _userManager.UpdateAsync(user);
        
        return user;
    }
}