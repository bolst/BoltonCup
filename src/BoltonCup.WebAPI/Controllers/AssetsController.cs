using BoltonCup.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class AssetsController(IAssetUploadService _uploadService) : BoltonCupControllerBase
{
    /// <remarks>
    /// Generates a pre-signed URL for uploads.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<PreSignedPutUrl>> GeneratePreSignedPutUrl(string fileExtension, string contentType)
    {
        return Ok(await _uploadService.GeneratePresignedPutUrl(fileExtension, contentType));
    }
}