using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
using BoltonCup.WebAPI.Mapping;
using static BoltonCup.WebAPI.Authentication.BoltonCupPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AccountsController(
    IPlayerRepository _players,
    IAccountRepository _accounts,
    IAccountService _accountService, 
    IUserService _userService,
    IAccountMapper _accountMapper,
    SignInManager<BoltonCupUser> _signInManager
) : BoltonCupControllerBase
{
    [Authorize]
    [HttpPost("complete-account")]
    public async Task<IActionResult> CompleteMyAccount([FromBody] CompleteUserAccountRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("ID claim is missing.");
        var command = _accountMapper.ToCommand(request, User);
        var user = await _userService.CompleteUserAccountAsync(userId, command);
        await _signInManager.RefreshSignInAsync(user);
        return Ok();
    }
    
    /// <remarks>
    /// Gets the currently logged-in user
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet("me")]
    public async Task<ActionResult<AccountDto>> GetMe()
    {
        var account = await _userService.GetMyAccountAsync(User);
        return OkOrNoContent(_accountMapper.ToDto(account, User));
    }

    [HttpPut("me")]
    public async Task<ActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
    {
        var command = _accountMapper.ToCommand(request, User);
        await _accountService.UpdateAsync(command);
        return NoContent();
    }

    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet("tournaments")]
    public async Task<ActionResult<ICollection<AccountTournamentDto>>> GetMyTournaments()
    {
        var accountId = User.GetAccountId();
        var account = await _accounts.GetByIdAsync(accountId);
        return Ok(_accountMapper.ToAccountTournamentDtoList(account));
    }
    
    /// <remarks>
    /// Updates an account's avatar by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPut("{id:int}/avatar")]
    public async Task<ActionResult> UpdateAvatar(int id, string tempKey)
    {
        await _accountService.UpdateAvatarAsync(id, tempKey);
        return Ok();
    }
    
    /// <remarks>
    /// Updates an account's banner by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPut("{id:int}/banner")]
    public async Task<ActionResult> UpdateBanner(int id, string tempKey)
    {
        await _accountService.UpdateBannerAsync(id, tempKey);
        return Ok();
    }
}
