using BoltonCup.Core;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AssetsController(IStorageService _storageService) : BoltonCupControllerBase
{
    /// <remarks>
    /// Generates a pre-signed URL for uploads.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<PreSignedPutUrl>> GeneratePreSignedPutUrl(string fileExtension, string contentType)
    {
        return Ok(await _storageService.GeneratePreSignedPutUrl(fileExtension, contentType));
    }
}