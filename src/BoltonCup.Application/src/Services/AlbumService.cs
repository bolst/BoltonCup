using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class AlbumService(
    IDbContextFactory<BoltonCupDbContext> _dbContextFactory,
    IStorageService _storageService,
    IAssetKeyGenerator _assetKeyGenerator) : IAlbumService
{
    public async Task<IPagedList<Album>> GetPublishedAsync(GetAlbumsQuery query, CancellationToken cancellationToken = default)
    {
        var label = string.IsNullOrWhiteSpace(query.Label) ? null : query.Label.Trim().ToLowerInvariant();

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await WithTags(db.Albums.AsNoTracking())
            .Include(a => a.CoverImage)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).Take(1))
            .Where(a => a.IsPublished)
            .ConditionalWhere(a => a.Tags.Any(t => t.Label != null && t.Label.Name.ToLower() == label), label is not null)
            .ApplySorting(
                query,
                x => x
                    .OrderByDescending(a => a.OccurredAt)
                    .ThenByDescending(a => a.Id))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<Album?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await WithTags(db.Albums.AsNoTracking())
            .Include(a => a.CoverImage)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)).ThenInclude(i => i.Tags).ThenInclude(t => t.Label)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)).ThenInclude(i => i.Tags).ThenInclude(t => t.Team)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)).ThenInclude(i => i.Tags).ThenInclude(t => t.Tournament)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)).ThenInclude(i => i.Tags).ThenInclude(t => t.Account)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)).ThenInclude(i => i.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.HomeTeam)
            .Include(a => a.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id)).ThenInclude(i => i.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.AwayTeam)
            .FirstOrDefaultAsync(a => a.IsPublished && a.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ReserveSlugsAsync(IReadOnlyList<SlugCandidate> candidates, CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await SlugReservation.ReserveAsync(
            candidates,
            ids => db.Albums
                .AsNoTracking()
                .Where(a => !ids.Contains(a.Id))
                .Select(a => a.Slug),
            cancellationToken);
    }

    public async Task<AlbumImage> AddImageAsync(int albumId, Stream content, string extension, string contentType, CancellationToken cancellationToken = default)
    {
        var key = _assetKeyGenerator.GenerateFinalKey<Album>(albumId.ToString(), "images", extension);
        await _storageService.PutAssetAsync(content, key, contentType, cancellationToken);

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var maxSortOrder = await db.AlbumImages
            .Where(i => i.AlbumId == albumId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var image = new AlbumImage
        {
            AlbumId = albumId,
            Key = key,
            SortOrder = maxSortOrder + 1,
        };

        db.AlbumImages.Add(image);
        await db.SaveChangesAsync(cancellationToken);
        return image;
    }

    public async Task DeleteImageAsync(int imageId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await db.AlbumImages.Where(i => i.Id == imageId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SetCoverImageAsync(int albumId, int? imageId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var album = await db.Albums.FirstOrDefaultAsync(a => a.Id == albumId, cancellationToken)
            ?? throw new InvalidOperationException($"Album {albumId} does not exist.");

        if (imageId is not null)
        {
            var belongsToAlbum = await db.AlbumImages.AnyAsync(i => i.Id == imageId && i.AlbumId == albumId, cancellationToken);
            if (!belongsToAlbum)
            {
                throw new InvalidOperationException($"Image {imageId} does not belong to album {albumId}.");
            }
        }

        album.CoverImageId = imageId;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderImagesAsync(int albumId, IReadOnlyList<int> orderedImageIds, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var images = await db.AlbumImages
            .Where(i => i.AlbumId == albumId)
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        for (var index = 0; index < orderedImageIds.Count; index++)
        {
            if (images.TryGetValue(orderedImageIds[index], out var image))
            {
                image.SortOrder = index;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ApplyAlbumTagsToImagesAsync(int albumId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var album = await db.Albums
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == albumId, cancellationToken)
            ?? throw new InvalidOperationException($"Album {albumId} does not exist.");

        var images = await db.AlbumImages
            .Where(i => i.AlbumId == albumId)
            .Include(i => i.Tags)
            .ToListAsync(cancellationToken);

        var albumTargets = album.Tags
            .Select(t => (Type: TagTargets.GetTargetType(t), Tag: t))
            .Where(x => x.Type is not null)
            .Select(x => (Type: x.Type!.Value, TargetId: TagTargets.GetTargetId(x.Tag, x.Type.Value)!.Value))
            .ToList();

        var added = 0;
        foreach (var image in images)
        {
            var existing = image.Tags
                .Select(t => (Type: TagTargets.GetTargetType(t), Tag: t))
                .Where(x => x.Type is not null)
                .Select(x => (Type: x.Type!.Value, TargetId: TagTargets.GetTargetId(x.Tag, x.Type.Value)!.Value))
                .ToHashSet();

            foreach (var (type, targetId) in albumTargets)
            {
                if (existing.Contains((type, targetId)))
                {
                    continue;
                }

                var tag = new AlbumImageTag { SubjectId = image.Id };
                TagTargets.SetTargetId(tag, type, targetId);
                db.AlbumImageTags.Add(tag);
                added++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return added;
    }

    static IQueryable<Album> WithTags(IQueryable<Album> albums) => albums
        .AsSplitQuery()
        .Include(a => a.Tags).ThenInclude(t => t.Label)
        .Include(a => a.Tags).ThenInclude(t => t.Team)
        .Include(a => a.Tags).ThenInclude(t => t.Tournament)
        .Include(a => a.Tags).ThenInclude(t => t.Account)
        .Include(a => a.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.HomeTeam)
        .Include(a => a.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.AwayTeam);
}