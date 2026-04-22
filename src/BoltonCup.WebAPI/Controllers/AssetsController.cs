using BoltonCup.Core;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AssetsController(IStorageService _storageService) : BoltonCupControllerBase
{
    /// <remarks>
    /// Generates a pre-signed URL and temporary key for uploads.
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet]
    public async Task<ActionResult<UploadCredentials>> GenerateUploadCredentials(string fileExtension, string contentType)
    {
        return Ok(await _storageService.GenerateUploadCredentialsAsync(fileExtension, contentType));
    }
}