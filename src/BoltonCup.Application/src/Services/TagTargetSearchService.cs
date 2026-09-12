using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Queries.Base;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

/// <summary>
/// The single place per-target-type search is configured, so a tag picker never needs
/// search expressions passed in from the page.
/// </summary>
class TagTargetSearchService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory) : ITagTargetSearchService
{
    const int MaxResults = 10;

    public async Task<IReadOnlyList<TagTargetOption>> SearchAsync(
        TagTargetType type,
        string? term,
        IReadOnlyCollection<int> excludeIds,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // No default arm: a new TagTargetType must produce a compiler warning here rather than
        // a picker that silently returns nothing.
        return type switch
        {
            TagTargetType.Game => await QueryAsync(
                db.Games.AsNoTracking().Include(g => g.HomeTeam).Include(g => g.AwayTeam),
                g => (g.HomeTeam != null ? g.HomeTeam.Name : " ") + ' ' + (g.AwayTeam != null ? g.AwayTeam.Name : " "),
                g => g.Id, excludeIds, term, cancellationToken),

            TagTargetType.Account => await QueryAsync(
                db.Accounts.AsNoTracking(),
                a => a.FirstName + ' ' + a.LastName,
                a => a.Id, excludeIds, term, cancellationToken),

            TagTargetType.Team => await QueryAsync(
                db.Teams.AsNoTracking(), t => t.Name, t => t.Id, excludeIds, term, cancellationToken),

            TagTargetType.Tournament => await QueryAsync(
                db.Tournaments.AsNoTracking(), t => t.Name, t => t.Id, excludeIds, term, cancellationToken),

            TagTargetType.Label => await QueryAsync(
                db.TagLabels.AsNoTracking(), l => l.Name, l => l.Id, excludeIds, term, cancellationToken),
        };
    }

    static async Task<IReadOnlyList<TagTargetOption>> QueryAsync<TEntity>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, string?>> searchBy,
        Func<TEntity, int> idSelector,
        IReadOnlyCollection<int> excludeIds,
        string? term,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var page = await query
            .WhereContains(searchBy, term)
            .OrderBy(searchBy)
            .ToPagedListAsync(new QueryBase { Size = MaxResults + excludeIds.Count }, cancellationToken);

        return page.Items
            .Where(e => !excludeIds.Contains(idSelector(e)))
            .Take(MaxResults)
            .Select(e => new TagTargetOption(idSelector(e), e.ToString() ?? string.Empty))
            .ToList();
    }
}
