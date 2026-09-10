using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class HighlightRepository(BoltonCupDbContext _context) : IHighlightRepository
{
    public async Task<IPagedList<Highlight>> GetAllAsync(GetHighlightsQuery query, CancellationToken cancellationToken = default) => await _context.Highlights
            .AsNoTracking()
            .Include(h => h.Tags)
                .ThenInclude(t => t.Game)
                    .ThenInclude(g => g!.Tournament)
            .Include(h => h.Tags)
                .ThenInclude(t => t.Player)
                    .ThenInclude(p => p!.Account)
            .Where(h => h.VideoId != null && h.VideoId != "")
            .Where(h => h.Tags.Any(t => t.GameId != null))
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
                .ThenInclude(t => t.Player)
                    .ThenInclude(p => p!.Account)
            .Where(h => h.Tags.Any(t => t.GameId == gameId))
            .OrderByDescending(h => h.CreatedAt)
            .ThenByDescending(h => h.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
}
