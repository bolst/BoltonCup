using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Infrastructure.Services;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages the authenticated user's account details.</summary>
public class AccountsController(
    IAccountRepository _accounts,
    IAccountService _accountService,
    IUserService _userService,
    IMapper _mapper,
    SignInManager<BoltonCupUser> _signInManager
) : BoltonCupControllerBase
{
    /// <summary>Completes the account setup for the currently logged-in user.</summary>
    [Authorize]
    [HttpPost("complete-account")]
    public async Task<IActionResult> CompleteMyAccount([FromBody] CompleteUserAccountRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("ID claim is missing.");

        var command = _mapper.ToCommand(request, User);
        var user = await _userService.CompleteUserAccountAsync(userId, command);
        await _signInManager.RefreshSignInAsync(user);
        return Ok();
    }

    /// <summary>Gets the currently logged-in user's account.</summary>
    /// <remarks>
    /// Gets the currently logged-in user
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet("me")]
    public async Task<ActionResult<AccountDto>> GetMe()
    {
        var account = await _userService.GetMyAccountAsync(User);
        return OkOrNoContent(_mapper.ToDto(account, User));
    }

    /// <summary>Updates the authenticated user's account details.</summary>
    [HttpPut("me")]
    public async Task<ActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
    {
        var command = _mapper.ToCommand(request, User);
        await _accountService.UpdateAsync(command);
        return NoContent();
    }

    /// <summary>Gets the tournaments associated with the authenticated user's account.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet("tournaments")]
    public async Task<ActionResult<ICollection<AccountTournamentDto>>> GetMyTournaments()
    {
        var accountId = User.GetAccountId();
        var account = await _accounts.GetByIdAsync(accountId);
        return Ok(_mapper.ToAccountTournamentDtoList(account));
    }

    /// <summary>Updates the authenticated user's avatar using a pre-signed S3 key.</summary>
    /// <remarks>
    /// Updates an account's avatar by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPut("avatar")]
    public async Task<ActionResult> UpdateAvatar(string tempKey)
    {
        var accountId = User.GetAccountId();
        await _accountService.UpdateAvatarAsync(accountId, tempKey);
        return Ok();
    }

    /// <summary>Updates the authenticated user's banner using a pre-signed S3 key.</summary>
    /// <remarks>
    /// Updates an account's banner by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPut("banner")]
    public async Task<ActionResult> UpdateBanner(string tempKey)
    {
        var accountId = User.GetAccountId();
        await _accountService.UpdateBannerAsync(accountId, tempKey);
        return Ok();
    }
}