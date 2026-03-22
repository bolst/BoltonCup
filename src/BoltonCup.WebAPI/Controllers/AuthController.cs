using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
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
    UserManager<BoltonCupUser> _userManager, 
    SignInManager<BoltonCupUser> _signInManager
    ) : BoltonCupControllerBase
{
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginRequest request)
    {
        // TODO: refactor this into user service
        
        // check if user exists
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Results.Unauthorized();
        // check if password is correct
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return Results.Unauthorized();
        // check if account is confirmed/verified
        if (!await _signInManager.CanSignInAsync(user))
            return Results.Forbid();
        
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.Persist, false);
        return result.Succeeded
            ? Results.Ok()
            : Results.Unauthorized();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request.Email, request.Password);
        return result.Succeeded
            ? Results.Ok()
            : Results.BadRequest(result.Errors.Select(e => e.Description));
    }

    [AllowAnonymous]
    [HttpPost("resendConfirmationEmail")]
    public async Task<IResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
    {
        await _userService.ResendConfirmationEmailAsync(request.Email);
        return Results.Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("verifyPasswordResetCode")]
    public async Task<IActionResult> VerifyPasswordResetCode([FromBody] VerifyPasswordResetCodeRequest request)
    {
        var isValid = await _userService.VerifyPasswordResetCodeAsync(request.Email, request.Code);
        if (!isValid)
            return BadRequest("Invalid code or email.");
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _userService.ForgotPasswordAsync(request.Email);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("resetPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InvalidPasswordResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword);
        return result.Succeeded
            ? Ok()
            : BadRequest(new InvalidPasswordResponse(result.Errors.Select(e => e.Description)));
    }

    [AllowAnonymous]
    [HttpPost("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var result = await _userService.ConfirmEmailAsync(request.Email, request.Code);
        return result.Succeeded
            ? Ok()
            : BadRequest("Invalid code or email.");
    }
    
    [HttpPost("logout")]
    public async Task<IResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Results.Ok();
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

public record InvalidPasswordResponse(IEnumerable<string> Errors);