namespace BoltonCup.Core.Interfaces;

public interface IAssetUploadService
{
    Task<PreSignedPutUrl> GeneratePresignedPutUrl(string fileExtension, string contentType);
}

public record PreSignedPutUrl(string UploadUrl, string TempKey);