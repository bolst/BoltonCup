using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class NewsPostService(
    BoltonCupDbContext _context,
    IStorageService _storageService,
    IAssetKeyGenerator _assetKeyGenerator) : INewsPostService
{
    public async Task<IPagedList<NewsPost>> GetPublishedAsync(GetNewsPostsQuery query, CancellationToken cancellationToken = default)
    {
        var label = string.IsNullOrWhiteSpace(query.Label) ? null : query.Label.Trim().ToLowerInvariant();

        return await WithTags(_context.NewsPosts.AsNoTracking())
            // One query per collection keeps the wide post row from repeating once per tag.
            .AsSplitQuery()
            .Where(p => p.IsPublished)
            .ConditionalWhere(p => p.Tags.Any(t => t.Label != null && t.Label.Name.ToLower() == label), label is not null)
            .ApplySorting(
                query,
                x => x
                    .OrderByDescending(p => p.PublishedAt)
                    .ThenByDescending(p => p.Id))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<NewsPost?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default) => await WithTags(_context.NewsPosts.AsNoTracking())
            .FirstOrDefaultAsync(p => p.IsPublished && p.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<string>> ReserveSlugsAsync(IReadOnlyList<SlugCandidate> candidates, CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var candidateIds = candidates.Select(c => c.PostId).Where(id => id != 0).ToList();
        var bases = candidates.Select(c => c.Slug).Distinct().ToList();

        // Every slug that could collide with a base or one of its numbered variants, owned by someone else.
        var taken = (await _context.NewsPosts
                .AsNoTracking()
                .Where(p => !candidateIds.Contains(p.Id))
                .Select(p => p.Slug)
                .ToListAsync(cancellationToken))
            .Where(slug => bases.Any(b => slug == b || slug.StartsWith(b + "-")))
            .ToHashSet();

        var reserved = new List<string>(candidates.Count);
        foreach (var candidate in candidates)
        {
            var slug = candidate.Slug;
            for (var suffix = 2; taken.Contains(slug); suffix++)
            {
                slug = SlugGenerator.WithSuffix(candidate.Slug, suffix);
            }

            taken.Add(slug);
            reserved.Add(slug);
        }

        return reserved;
    }

    public Task UpdateCoverImageAsync(int id, string tempKey, CancellationToken cancellationToken = default) => _storageService.UpdateAssetAsync<NewsPost>(
            _context,
            _assetKeyGenerator,
            p => p.Id == id,
            p => p.CoverImage,
            tempKey,
            id.ToString(),
            cancellationToken);

    // Every tag target the API renders a name for. Kept in one place so list and detail agree.
    static IQueryable<NewsPost> WithTags(IQueryable<NewsPost> posts) => posts
        .Include(p => p.Tags).ThenInclude(t => t.Label)
        .Include(p => p.Tags).ThenInclude(t => t.Team)
        .Include(p => p.Tags).ThenInclude(t => t.Tournament)
        .Include(p => p.Tags).ThenInclude(t => t.Account)
        .Include(p => p.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.HomeTeam)
        .Include(p => p.Tags).ThenInclude(t => t.Game).ThenInclude(g => g!.AwayTeam);
}
