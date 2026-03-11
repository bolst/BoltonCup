namespace BoltonCup.Core;

public interface IAssetUploadService
{
    Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType, CancellationToken cancellationToken = default);
    Task CopyAssetAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);
}

public record PreSignedPutUrl(string UploadUrl, string TempKey);