using BoltonCup.WebAPI.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("/api/auth")]
[Tags("Auth")]
public class AuthController(
    UserManager<IdentityUser> _userManager, 
    SignInManager<IdentityUser> _signInManager
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
    
    [HttpPost("logout")]
    public async Task<IResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Results.Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("user")]
    public async Task<ActionResult<UserDto?>> GetUser([FromBody] string email)
    {
        // Simply check if the user exists in the DB
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound();
        return Ok(new UserDto
        {
            Email = user.Email,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber,
        });
    }
}

public class UserDto
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class LoginWithCookieRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}