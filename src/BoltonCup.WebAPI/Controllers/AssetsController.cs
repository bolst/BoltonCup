using BoltonCup.Core;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AssetsController(IStorageService _storageService) : BoltonCupControllerBase
{
    /// <remarks>
    /// Generates a pre-signed URL and temporary key for uploads.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<UploadCredentials>> GenerateUploadCredentials(string fileExtension, string contentType)
    {
        return Ok(await _storageService.GenerateUploadCredentialsAsync(fileExtension, contentType));
    }
}