namespace BoltonCup.Core;

public interface IAssetUploadService
{
    Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType);
}

public record PreSignedPutUrl(string UploadUrl, string TempKey);