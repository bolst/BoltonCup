using Amazon.S3;
using Amazon.S3.Model;
using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

public class ServerAssetUploadService(IAmazonS3 _s3Client) 
    : IAssetUploadService
{
    private const string _bucketName = "bolton-cup-assets";
    private const string _prefix = "temp-uploads/";

    public async Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType, CancellationToken cancellationToken = default)
    {
        var tempKey = $"{_prefix}{Guid.NewGuid()}{fileExtension}";
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

    public async Task CommitAsync<TEntity>(AssetCommitCommand<TEntity> command,
        CancellationToken cancellationToken = default) where TEntity : EntityBase
    {
        if (!command.TempKey.StartsWith(_prefix))
            throw new InvalidOperationException("Attempted to commit a non-ephemeral asset.");
        
        var destination = command.Destination.Compile().Invoke(command.Entity);
        await _s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceKey = command.TempKey,
            SourceBucket = _bucketName,
            DestinationKey = destination,
            DestinationBucket = _bucketName, 
        }, cancellationToken);
    }
}