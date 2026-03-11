using BoltonCup.Core;
using BoltonCup.Sdk;
using PreSignedPutUrl = BoltonCup.Core.PreSignedPutUrl;

namespace BoltonCup.Common.Services;

public class WasmAssetUploadService(IBoltonCupApi _boltonCupApi) 
    : IAssetUploadService
{
    public async Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType, CancellationToken cancellationToken = default)
    { 
        var result = await _boltonCupApi.GeneratePreSignedPutUrlAsync(fileExtension, contentType, cancellationToken);
        return new PreSignedPutUrl(result.UploadUrl, result.TempKey);
    }

    public Task CopyAssetAsync(string sourceKey, string destinationKey,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}