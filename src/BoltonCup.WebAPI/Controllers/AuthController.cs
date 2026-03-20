using System.Diagnostics.CodeAnalysis;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
using BoltonCup.WebAPI.Mapping.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("/api/auth")]
[Tags("Auth")]
public class AuthController(
    IUserService _userService,
    UserManager<BoltonCupUser> _userManager, 
    SignInManager<BoltonCupUser> _signInManager,
    IBoltonCupUserMapper _userMapper
    ) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets the currently logged-in user
    /// </remarks>
    [HttpGet("me")]
    public ActionResult<UserInfoDto> GetMe()
    {
        return new UserInfoDto(User);
    }

    [AllowAnonymous]
    [HttpPost("login-cookie")]
    public async Task<IResult> LoginWithCookie([FromBody] LoginWithCookieRequest request)
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
    [HttpPost("registerV2")]
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
    [HttpPost("verifyResetCode")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyCodeRequest request)
    {
        var isValid = await _userService.VerifyPasswordResetCodeAsync(request.Email, request.Code);
        if (!isValid)
            return BadRequest("Invalid code or email.");
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("forgotPasswordV2")]
    public async Task<IActionResult> ForgotPasswordV2([FromBody] ForgotPasswordV2Request request)
    {
        await _userService.ForgotPasswordV2Async(request.Email);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("resetPasswordV2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InvalidPasswordResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordV2([FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordV2Async(request.Email, request.ResetCode, request.NewPassword);
        return result.Succeeded
            ? Ok()
            : BadRequest(new InvalidPasswordResponse(result.Errors.Select(e => e.Description)));
    }

    [AllowAnonymous]
    [HttpPost("confirmEmailV2")]
    public async Task<IActionResult> ConfirmEmailV2([FromBody] ConfirmEmailV2Request request)
    {
        var result = await _userService.ConfirmEmailV2Async(request.Email, request.Code);
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
    [HttpPost("user")]
    public async Task<ActionResult<UserDto>> GetUser([FromBody] string email)
    {
        // Simply check if the user exists in the DB
        var user = await _userManager.FindByEmailAsync(email);
        return OkOrNotFound(_userMapper.ToDto(user));
    }
}

public record VerifyCodeRequest(string Email, string Code);

public record ForgotPasswordV2Request(string Email);

public record ConfirmEmailV2Request(string Email, string Code);

public record LoginWithCookieRequest(string Email, string Password, bool Persist = true);

public record InvalidPasswordResponse(IEnumerable<string> Errors);