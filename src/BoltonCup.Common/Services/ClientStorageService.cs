using BoltonCup.Core;
using BoltonCup.Sdk;

namespace BoltonCup.Common.Services;

public class ClientStorageService(IBoltonCupApi _boltonCupApi) 
    : IStorageService
{
    public async Task<UploadCredentials> GenerateUploadCredentialsAsync(string fileExtension, string contentType, CancellationToken cancellationToken = default)
    { 
        var result = await _boltonCupApi.GeneratePreSignedPutUrlAsync(fileExtension, contentType, cancellationToken);
        return new UploadCredentials(result.UploadUrl, result.TempKey);
    }

    public Task CopyAssetAsync(string sourceKey, string destinationKey,
        CancellationToken cancellationToken = default)
    {
        // TODO
        return Task.CompletedTask;
    }
}