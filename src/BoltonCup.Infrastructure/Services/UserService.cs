using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Services;

public interface IUserService
{
    Task<bool> VerifyPasswordResetCodeAsync(string email, string code);
    Task<IdentityResult> ResetPasswordV2Async(string email, string code, string newPassword);
}

public class UserService(UserManager<BoltonCupUser> _userManager) : IUserService
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

    public async Task<IdentityResult> ResetPasswordV2Async(string email, string code, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null
            ? IdentityResult.Failed(new IdentityError { Description = "Invalid request." })
            : await _userManager.ResetPasswordAsync(user, code, newPassword);
    }
}