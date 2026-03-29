using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
using BoltonCup.WebAPI.Mapping;
using BoltonCup.WebAPI.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BoltonCup.WebAPI.Controllers;

[Route("/api/auth")]
[Tags("Auth")]
public class AuthController(
    IUserService _userService,
    IUserMapper _userMapper,
    UserManager<BoltonCupUser> _userManager, 
    SignInManager<BoltonCupUser> _signInManager
    ) : BoltonCupControllerBase
{

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserDto?> GetCurrentUser()
    {
        var user = _userMapper.ToDto(User);
        return user is not null
            ? Ok(user)
            : Unauthorized();
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        await _userService.LoginAsync(request.Email, request.Password, request.Persist);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        await _userService.RegisterAsync(request.Email, request.Password);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("resendConfirmationEmail")]
    public async Task<ActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
    {
        await _userService.ResendConfirmationEmailAsync(request.Email);
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("verifyPasswordResetCode")]
    public async Task<ActionResult> VerifyPasswordResetCode([FromBody] VerifyPasswordResetCodeRequest request)
    {
        await _userService.VerifyPasswordResetCodeAsync(request.Email, request.Code);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("forgotPassword")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _userService.ForgotPasswordAsync(request.Email);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("resetPassword")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("confirmEmail")]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _userService.ConfirmEmailAsync(request.Email, request.Code);
        return Ok();
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IResult> Logout([FromQuery] string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return !string.IsNullOrWhiteSpace(returnUrl) 
            ? Results.Redirect(returnUrl) 
            : Results.Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("continue")]
    [EnableRateLimiting(nameof(StrictEmailCheckPolicy))]
    public async Task<ActionResult<bool>> CheckUserAsync([FromBody] CheckUserRequest request)
    {
        return await _userManager.FindByEmailAsync(request.Email) is not null;
    }
}

public record VerifyPasswordResetCodeRequest(string Email, string Code);

public record ForgotPasswordRequest(string Email);

public record ConfirmEmailRequest(string Email, string Code);

public record LoginRequest(string Email, string Password, bool Persist = true);

public record CheckUserRequest(string Email);