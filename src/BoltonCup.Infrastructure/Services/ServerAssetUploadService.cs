using Amazon.S3;
using Amazon.S3.Model;
using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

public class ServerAssetUploadService(IAmazonS3 _s3Client, IAssetKeyGenerator _keyGenerator) 
    : IAssetUploadService
{
    private const string _bucketName = "bolton-cup-assets";

    public async Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType,
        CancellationToken cancellationToken = default)
    {
        var tempKey = _keyGenerator.GenerateTempKey(fileExtension);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = tempKey,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Verb = HttpVerb.PUT,
            ContentType = contentType
        };
        var url = await _s3Client.GetPreSignedURLAsync(request);
        return new PreSignedPutUrl(url, tempKey);
    }

    public Task CopyAssetAsync(string sourceKey, string destinationKey,
        CancellationToken cancellationToken = default)
    {
        _keyGenerator.ThrowIfNotValidTempKey(sourceKey);
        return _s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceKey = sourceKey,
            SourceBucket = _bucketName,
            DestinationKey = destinationKey,
            DestinationBucket = _bucketName, 
        }, cancellationToken);
    }
}