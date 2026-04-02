using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TournamentRegistrationService(BoltonCupDbContext _dbContext) : ITournamentRegistrationService
{
    public async Task<TournamentRegistration?> GetAsync(int tournamentId, int accountId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TournamentRegistrations
            .AsNoTracking()
            .Include(e => e.Account)
            .Include(e => e.Tournament)
            .FirstOrDefaultAsync(t => t.AccountId == accountId && t.TournamentId == tournamentId,
                cancellationToken: cancellationToken);
    }

    public async Task UpsertAsync(UpsertTournamentRegistrationCommand command,
        CancellationToken cancellationToken = default)
    {
        var tournament = await _dbContext.Tournaments.FindAsync([command.TournamentId], cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);
        
        var entry = await _dbContext.TournamentRegistrations
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                cancellationToken);
        
        var addNew = entry is null;
        
        entry ??= new TournamentRegistration();
        entry.TournamentId = command.TournamentId;
        entry.AccountId = command.AccountId;
        entry.CurrentStep = command.CurrentStep;
        entry.Payload = command.Payload;
        entry.IsComplete = command.IsComplete;

        if (addNew)
            await _dbContext.TournamentRegistrations.AddAsync(entry, cancellationToken);
        else
            _dbContext.TournamentRegistrations.Update(entry);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}