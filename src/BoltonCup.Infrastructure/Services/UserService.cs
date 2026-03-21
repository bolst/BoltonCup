using System.ComponentModel.DataAnnotations;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Services;

public interface IUserService
{
    Task<IdentityResult> RegisterAsync(string email, string password);
    Task ResendConfirmationEmailAsync(string email);
    Task<bool> VerifyPasswordResetCodeAsync(string email, string code);
    Task ForgotPasswordAsync(string email);
    Task<IdentityResult> ResetPasswordAsync(string email, string code, string newPassword);
    Task<IdentityResult> ConfirmEmailAsync(string email, string code);
}

public class UserService(
    UserManager<BoltonCupUser> _userManager, 
    IEmailer _emailer) : IUserService
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    
    
    public async Task<IdentityResult> RegisterAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            return IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email));

        var user = new BoltonCupUser();
        await _userManager.SetUserNameAsync(user, email);
        await _userManager.SetEmailAsync(user, email);
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user); 
            await _emailer.SendConfirmationCodeAsync(user, email, code);
        }
        return result;
    }

    
    
    public async Task ResendConfirmationEmailAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return;
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailer.SendConfirmationCodeAsync(user, email, code);
    }
    
    
    
    public async Task<bool> VerifyPasswordResetCodeAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is not null 
               && await _userManager.VerifyUserTokenAsync(
                   user: user, 
                   tokenProvider: _userManager.Options.Tokens.PasswordResetTokenProvider,
                   purpose: UserManager<BoltonCupUser>.ResetPasswordTokenPurpose,
                   token: code
                );
    }

    
    
    public async Task ForgotPasswordAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return;
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailer.SendPasswordResetCodeAsync(user, email, code);
    }

    
    
    public async Task<IdentityResult> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Invalid request." });

        var result = await _userManager.ResetPasswordAsync(user, code, newPassword);
        // a user could only get here if they got the code via email
        // thus, confirm their account if needed
        if (result.Succeeded && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        return result;
    }

    
    
    public async Task<IdentityResult> ConfirmEmailAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Invalid request." });
        return await _userManager.ConfirmEmailAsync(user, code);
    }
}