using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GameHighlightRepository(BoltonCupDbContext _context) : IGameHighlightRepository
{
    public async Task<IPagedList<GameHighlight>> GetAllAsync(GetHighlightsQuery query, CancellationToken cancellationToken = default) => await _context.GameHighlights
            .AsNoTracking()
            .Include(h => h.Game)
                .ThenInclude(g => g.Tournament)
            .Include(h => h.Player)
                .ThenInclude(p => p!.Account)
            .Where(h => h.VideoId != null && h.VideoId != "")
            .ApplySorting(
                query,
                x => x
                    .OrderByDescending(h => h.CreatedAt)
                    .ThenByDescending(h => h.Id)
            )
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
}
