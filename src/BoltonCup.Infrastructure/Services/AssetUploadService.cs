using Amazon.S3;
using BoltonCup.Core.Interfaces;

namespace BoltonCup.Infrastructure.Services;

public class AssetUploadService : IAssetUploadService
{
    private readonly IAmazonS3 _s3Client;

    public AssetUploadService(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public Task<PreSignedPutUrl> GeneratePresignedPutUrl(string fileExtension, string contentType)
    {
        var tempKey = $"uploads/{Guid.NewGuid()}{fileExtension}";
        var uploadUrl = _s3Client.GeneratePreSignedURL(
            bucketName: "bolton-cup-temp-uploads",
            objectKey: tempKey,
            expiration: DateTime.UtcNow.AddMinutes(15),
            additionalProperties: null
        );
        return Task.FromResult(new PreSignedPutUrl(uploadUrl, tempKey));
    }
}