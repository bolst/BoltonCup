using System.Linq.Expressions;

namespace BoltonCup.Core;

public interface IAssetUploadService
{
    Task<PreSignedPutUrl> GeneratePreSignedPutUrl(string fileExtension, string contentType, CancellationToken cancellationToken = default);
    Task CommitAsync<TEntity>(AssetCommitCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : EntityBase;
}

public record PreSignedPutUrl(string UploadUrl, string TempKey);

public record AssetCommitCommand<TEntity> where TEntity : EntityBase
{
    public required TEntity Entity { get; init; }
    public required string TempKey { get; init; }
    public required Expression<Func<TEntity, string?>> Destination { get; init; }
}