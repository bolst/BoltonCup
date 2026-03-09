using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class InfoGuideRepository(BoltonCupDbContext _context) : IInfoGuideRepository
{
    public async Task<CollectionResult<InfoGuide>> GetAllAsync(GetInfoGuidesQuery query)
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .Include(e => e.Tournament)
            .ApplySorting(query, x => x
                .OrderByDescending(e => e.LastModified)
                .ThenByDescending(e => e.CreatedAt))
            .ToPaginatedListAsync(query);
    }
        
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetInfoGuidesQuery query)
        where T : IMappable<InfoGuide, T>
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .ApplySorting(query, x => x
                .OrderByDescending(e => e.LastModified)
                .ThenByDescending(e => e.CreatedAt))
            .ProjectTo<InfoGuide, T>()
            .ToPaginatedListAsync(query);
    }
    
    public async Task<InfoGuide?> GetByIdAsync(Guid id)
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .Include(e => e.Tournament)
            .OrderByDescending(e => e.LastModified)
            .ThenByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(Guid id)
        where T : IMappable<InfoGuide, T>
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<InfoGuide, T>(e => e.Id == id);
    }
    
    public async Task<InfoGuide?> GetByTournamentIdAsync(int tournamentId)
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .Include(e => e.Tournament)
            .FirstOrDefaultAsync(e => e.TournamentId == tournamentId);
    }

    public async Task<T?> GetByTournamentIdAsync<T>(int tournamentId)
        where T : IMappable<InfoGuide, T>
    {
        return await _context.InfoGuides
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<InfoGuide, T>(e => e.TournamentId == tournamentId);
    }

    public async Task<bool> AddAsync(InfoGuide entity)
    {
        await _context.InfoGuides.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(InfoGuide entity)
    {
        _context.InfoGuides.Update(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.InfoGuides.FindAsync(id);
        if (entity == null) return false;

        _context.InfoGuides.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}