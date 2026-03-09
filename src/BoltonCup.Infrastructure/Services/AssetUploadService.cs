using Amazon.S3;
using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

public class AssetUploadService(IAmazonS3 _s3Client) 
    : IAssetUploadService
{
    private const string _bucketName = "bolton-cup-assets";
    private const string _prefix = "temp-uploads";

    public Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType)
    {
        var tempKey = $"{_prefix}/{Guid.NewGuid()}{fileExtension}";
        var uploadUrl = _s3Client.GeneratePreSignedURL(
            bucketName: _bucketName,
            objectKey: tempKey,
            expiration: DateTime.UtcNow.AddMinutes(15),
            additionalProperties: null
        );
        return Task.FromResult(new PreSignedPutUrl(uploadUrl, tempKey));
    }
}