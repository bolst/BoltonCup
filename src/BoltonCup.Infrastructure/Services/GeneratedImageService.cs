using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class GeneratedImageService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory) : IGeneratedImageService
{
    public async Task<GeneratedImage> CreateAsync(string storageKey, string templateKey, string label,
        string contentType, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var image = new GeneratedImage
        {
            StorageKey = storageKey,
            TemplateKey = templateKey,
            Label = label,
            ContentType = contentType,
        };
        dbContext.GeneratedImages.Add(image);
        await dbContext.SaveChangesAsync(cancellationToken);
        return image;
    }

    public async Task<IReadOnlyList<GeneratedImage>> GetRecentAsync(int limit = 100,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.GeneratedImages
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}