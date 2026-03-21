using BoltonCup.Core;
using BoltonCup.Infrastructure.Services;
using BoltonCup.WebAPI.Mapping.Core;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AccountsController(
    IAccountService _accountService, 
    IUserService _userService, 
    IAccountMapper _accountMapper
) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets the currently logged-in user
    /// </remarks>
    [HttpGet("me")]
    public async Task<ActionResult<AccountDto>> GetMe()
    {
        var account = await _userService.GetMeAsync(User);
        return OkOrNotFound(_accountMapper.ToDto(account, User));
    }
    
    /// <remarks>
    /// Updates an account's avatar by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
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
    [HttpPut("{id:int}/banner")]
    public async Task<ActionResult> UpdateBanner(int id, string tempKey)
    {
        await _accountService.UpdateBannerAsync(id, tempKey);
        return Ok();
    }
}
