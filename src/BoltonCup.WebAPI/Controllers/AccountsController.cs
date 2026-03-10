using BoltonCup.Core;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AccountsController(IAccountService _accountService) : BoltonCupControllerBase
{
    /// <remarks>
    /// Updates an account's profile picture by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [HttpPut("{id:int}/profile-picture")]
    public async Task<ActionResult> UpdateProfilePicture(int id, string key)
    {
        await _accountService.UpdateProfilePictureAsync(id, key);
        return Ok();
    }
}
