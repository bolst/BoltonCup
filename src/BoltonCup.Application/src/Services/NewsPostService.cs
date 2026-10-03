using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class NewsPostService(
    IDbContextFactory<BoltonCupDbContext> _dbContextFactory,
    IStorageService _storageService,
    IAssetKeyGenerator _assetKeyGenerator) : INewsPostService
{
    public async Task<IPagedList<NewsPost>> GetPublishedAsync(GetNewsPostsQuery query, CancellationToken cancellationToken = default)
    {
        var label = string.IsNullOrWhiteSpace(query.Label) ? null : query.Label.Trim().ToLowerInvariant();

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await WithTags(db.NewsPosts.AsNoTracking())
            .Where(p => p.IsPublished)
            .ConditionalWhere(p => p.Tags.Any(t => t.Label != null && t.Label.Name.ToLower() == label), label is not null)
            .ApplySorting(
                query,
                x => x
                    .OrderByDescending(p => p.PublishedAt)
                    .ThenByDescending(p => p.Id))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<NewsPost?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await WithTags(db.NewsPosts.AsNoTracking())
            .FirstOrDefaultAsync(p => p.IsPublished && p.Slug == slug, cancellationToken);
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
            ids => db.NewsPosts
                .AsNoTracking()
                .Where(p => !ids.Contains(p.Id))
                .Select(p => p.Slug),
            cancellationToken);
    }

    public async Task UpdateCoverImageAsync(int id, string tempKey, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await _storageService.UpdateAssetAsync<NewsPost>(
            db,
            _assetKeyGenerator,
            p => p.Id == id,
            p => p.CoverImage,
            tempKey,
            id.ToString(),
            cancellationToken);
    }

    // Every tag target the API renders a name for. Kept in one place so list and detail agree.
    // Split so the wide post row (Markdown body included) is fetched once, not once per tag.
    static IQueryable<NewsPost> WithTags(IQueryable<NewsPost> posts) => posts
        .AsSplitQuery()
        .Include(p => p.Tags).ThenInclude(t => t.Label)
        .Include(p => p.Tags).ThenInclude(t => t.Team)
        .Include(p => p.Tags).ThenInclude(t => t.Tournament)
        .Include(p => p.Tags).ThenInclude(t => t.Account)
        .Include(p => p.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.HomeTeam)
        .Include(p => p.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.AwayTeam);
}
