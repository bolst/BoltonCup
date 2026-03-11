using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Extensions;

public static class IStorageServiceExtensions
{
    public static async Task UpdateAssetAsync<TEntity>(
        this IStorageService storageService,
        BoltonCupDbContext dbContext,
        IAssetKeyGenerator keyGenerator,
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity, string> updateAction,
        string tempKey,
        string identifier,
        string asset,
        CancellationToken cancellationToken = default
    ) where TEntity : EntityBase
    {
        var entity = await dbContext.Set<TEntity>()
                          .Where(predicate)
                          .FirstOrDefaultAsync(cancellationToken) 
                      ?? throw new InvalidOperationException($"Could not find any {typeof(TEntity).Name} entities with the given predicate.");
        // commit asset to final location in S3
        var extension = Path.GetExtension(tempKey);
        var destination = keyGenerator.GenerateFinalKey<TEntity>(identifier, asset, extension);
        await storageService.CopyAssetAsync(tempKey, destination, cancellationToken);
        // update account in db
        updateAction(entity, destination);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}