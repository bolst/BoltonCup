using System.Text;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace BoltonCup.Infrastructure.Services;

public interface IUserService
{
    Task<bool> VerifyPasswordResetCodeAsync(string email, string code);
    Task ForgotPasswordV2Async(string email);
    Task<IdentityResult> ResetPasswordV2Async(string email, string code, string newPassword);
}

public class UserService(UserManager<BoltonCupUser> _userManager, IEmailSender<BoltonCupUser> _emailSender) : IUserService
{
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

    public async Task ForgotPasswordV2Async(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return;
        // get code and encode it to base64
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedBytes = Encoding.UTF8.GetBytes(code);
        var encodedCode = WebEncoders.Base64UrlEncode(encodedBytes);
        // send email
        await _emailSender.SendPasswordResetCodeAsync(user, email, encodedCode);
    }

    public async Task<IdentityResult> ResetPasswordV2Async(string email, string code, string newPassword)
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
}