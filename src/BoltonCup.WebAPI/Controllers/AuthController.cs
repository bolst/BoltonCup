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
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, true, false);
        return result.Succeeded
            ? Results.Ok()
            : Results.Unauthorized();
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
    [HttpPost("/resetPasswordV2")]
    public async Task<IActionResult> ResetPasswordV2([FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordV2Async(request.Email, request.ResetCode, request.NewPassword);
        return result.Succeeded
            ? Ok()
            : BadRequest( new { Errors = result.Errors.Select(e => e.Description) });
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

public record LoginWithCookieRequest(string Email, string Password);
