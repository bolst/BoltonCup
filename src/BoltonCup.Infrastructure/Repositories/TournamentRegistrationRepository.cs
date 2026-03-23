using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TournamentRegistrationRepository(BoltonCupDbContext _dbContext) : ITournamentRegistrationRepository
{
    public async Task<TournamentRegistration?> GetAsync(int tournamentId, int accountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TournamentRegistrations
            .AsNoTracking()
            .Include(e => e.Account)
            .Include(e => e.Tournament)
            .FirstOrDefaultAsync(t => t.AccountId == accountId && t.TournamentId == tournamentId, cancellationToken: cancellationToken);
    }
    
    public async Task<bool> AddAsync(TournamentRegistration entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.TournamentRegistrations.AddAsync(entity, cancellationToken);
        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
    
    public async Task<bool> UpdateAsync(TournamentRegistration entity, CancellationToken cancellationToken = default)
    {
        _dbContext.TournamentRegistrations.Update(entity);
        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}