using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Extensions;

public static class UpdateAssetAsync
{ 
    public static async Task UpdateSingleAssetAsync<TEntity>(
        this IAssetUploadService assetUploadService,
        BoltonCupDbContext dbContext,
        string tempKey,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, string?>> destination,
        CancellationToken cancellationToken = default
    ) where TEntity : EntityBase 
    {
        var entity = await dbContext
            .Set<TEntity>()
            .FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        if (entity == null)
            throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} not found.");
        
        var command = new AssetCommitCommand<TEntity>
        {
            Entity = entity,
            TempKey = tempKey,
            Destination = destination,
        };
        await assetUploadService.CommitAsync(command, cancellationToken);
    }
}