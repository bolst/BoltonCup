using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using BoltonCup.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;



class HighlightService(BoltonCupDbContext _context) : IHighlightService
{
    public async Task<IPagedList<Highlight>> GetAllAsync(GetHighlightsQuery query, CancellationToken cancellationToken = default) => await _context.Highlights
            .AsNoTracking()
            .Include(h => h.Tags)
                .ThenInclude(t => t.Game)
                    .ThenInclude(g => g!.Tournament)
            .Include(h => h.Tags)
                .ThenInclude(t => t.Account)
            .Where(h => h.VideoId != null && h.VideoId != "")
            .ConditionalWhere(h => h.Tags.Any(t => t.AccountId == query.AccountId), query.AccountId.HasValue)
            .ApplySorting(
                query,
                x => x
                    .OrderByDescending(h => h.CreatedAt)
                    .ThenByDescending(h => h.Id)
            )
            .ToPagedListAsync(query, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Highlight>> GetForGameAsync(int gameId, int take, CancellationToken cancellationToken = default) => await _context.Highlights
            .AsNoTracking()
            .Include(h => h.Tags)
                .ThenInclude(t => t.Account)
            .Where(h => h.Tags.Any(t => t.GameId == gameId))
            .OrderByDescending(h => h.CreatedAt)
            .ThenByDescending(h => h.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
}
