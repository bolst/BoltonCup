using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class InfoGuideRepository(BoltonCupDbContext _context) : IInfoGuideRepository
{
    public async Task<IPagedList<InfoGuide>> GetAllAsync(GetInfoGuidesQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .Include(e => e.Tournament)
            .ApplySorting(query, x => x
                .OrderByDescending(e => e.LastModified)
                .ThenByDescending(e => e.CreatedAt))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }
        
    public async Task<InfoGuide?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .Include(e => e.Tournament)
            .OrderByDescending(e => e.LastModified)
            .ThenByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<InfoGuide?> GetByTournamentIdAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .Include(e => e.Tournament)
            .FirstOrDefaultAsync(e => e.TournamentId == tournamentId, cancellationToken: cancellationToken);
    }
}