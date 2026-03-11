namespace BoltonCup.Core;

public interface IStorageService
{
    Task<UploadCredentials> GenerateUploadCredentialsAsync(string fileExtension, string contentType, CancellationToken cancellationToken = default);
    Task CopyAssetAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);
}

public record UploadCredentials(string UploadUrl, string TempKey);