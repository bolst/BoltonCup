using BoltonCup.Core;
using BoltonCup.Sdk;

namespace BoltonCup.Common.Services;

public class ClientStorageService(IBoltonCupApi _boltonCupApi) 
    : IStorageService
{
    public async Task<Core.UploadCredentials> GenerateUploadCredentialsAsync(string fileExtension, string contentType, CancellationToken cancellationToken = default)
    { 
        var result = await _boltonCupApi.GenerateUploadCredentialsAsync(fileExtension, contentType, cancellationToken);
        return new Core.UploadCredentials(result.UploadUrl, result.TempKey);
    }

    public Task CopyAssetAsync(string sourceKey, string destinationKey,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("ClientStorageService does not support copying assets. This method should not be called on the client.");
    }
}