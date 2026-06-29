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

/// <summary>Handles user authentication including login, registration, password reset, and email confirmation.</summary>
[Route("/api/auth")]
[Tags("Auth")]
public class AuthController(
    IUserService _userService,
    IMapper _mapper,
    UserManager<BoltonCupUser> _userManager,
    SignInManager<BoltonCupUser> _signInManager
) : BoltonCupControllerBase
{
    /// <summary>Returns the currently authenticated user's identity information.</summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserDto?> GetCurrentUser()
    {
        var user = _mapper.ToDto(User);
        return user is not null
            ? Ok(user)
            : Unauthorized();
    }

    /// <summary>Authenticates a user with email and password.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        await _userService.LoginAsync(request.Email, request.Password, request.Persist);
        return Ok();
    }

    /// <summary>Registers a new user account.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        await _userService.RegisterAsync(request.Email, request.Password);
        return Ok();
    }

    /// <summary>Resends the email confirmation link to the specified address.</summary>
    [AllowAnonymous]
    [HttpPost("resendConfirmationEmail")]
    public async Task<ActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
    {
        await _userService.ResendConfirmationEmailAsync(request.Email);
        return Ok();
    }

    /// <summary>Verifies a password reset code without applying the reset.</summary>
    [AllowAnonymous]
    [HttpPost("verifyPasswordResetCode")]
    public async Task<ActionResult> VerifyPasswordResetCode([FromBody] VerifyPasswordResetCodeRequest request)
    {
        await _userService.VerifyPasswordResetCodeAsync(request.Email, request.Code);
        return Ok();
    }

    /// <summary>Initiates the forgot-password flow by sending a reset email.</summary>
    [AllowAnonymous]
    [HttpPost("forgotPassword")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _userService.ForgotPasswordAsync(request.Email);
        return Ok();
    }

    /// <summary>Resets the user's password using a valid reset code.</summary>
    [AllowAnonymous]
    [HttpPost("resetPassword")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword);
        return Ok();
    }

    /// <summary>Confirms the user's email address using the code sent during registration.</summary>
    [AllowAnonymous]
    [HttpPost("confirmEmail")]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _userService.ConfirmEmailAsync(request.Email, request.Code);
        return Ok();
    }

    /// <summary>Signs the current user out and optionally redirects to a return URL.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IResult> Logout([FromQuery] string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return !string.IsNullOrWhiteSpace(returnUrl)
            ? Results.Redirect(returnUrl)
            : Results.Ok();
    }

    /// <summary>Checks whether a user account exists for the given email address.</summary>
    [AllowAnonymous]
    [HttpPost("continue")]
    [EnableRateLimiting(nameof(StrictEmailCheckPolicy))]
    public async Task<ActionResult<bool>> CheckUserAsync([FromBody] CheckUserRequest request)
    {
        return await _userManager.FindByEmailAsync(request.Email) is not null;
    }
}
/// <summary>Request payload for verifying a password reset code.</summary>
public record VerifyPasswordResetCodeRequest(string Email, string Code);
/// <summary>Request payload for initiating a forgot-password flow.</summary>
public record ForgotPasswordRequest(string Email);
/// <summary>Request payload for confirming an email address.</summary>
public record ConfirmEmailRequest(string Email, string Code);
/// <summary>Request payload for authenticating a user.</summary>
public record LoginRequest(string Email, string Password, bool Persist = true);
/// <summary>Request payload for checking whether an account exists for an email.</summary>
public record CheckUserRequest(string Email);