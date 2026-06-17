using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TournamentPlayerInfoService(BoltonCupDbContext _dbContext) : ITournamentPlayerInfoService
{
    public async Task<TournamentPlayerInfoContext> GetAsync(int tournamentId, int accountId,
        CancellationToken cancellationToken = default)
    {
        var info = await _dbContext.TournamentPlayerInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AccountId == accountId && e.TournamentId == tournamentId, cancellationToken);

        var player = await _dbContext.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccountId == accountId && p.TournamentId == tournamentId, cancellationToken);

        if (player?.TeamId is not int teamId)
            return new TournamentPlayerInfoContext(info, []);

        var teamGames = await _dbContext.Games
            .AsNoTracking()
            .Include(g => g.Tournament)
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .Where(g => g.TournamentId == tournamentId && (g.HomeTeamId == teamId || g.AwayTeamId == teamId))
            .OrderBy(g => g.GameTime)
            .ToListAsync(cancellationToken);

        return new TournamentPlayerInfoContext(info, teamGames);
    }

    public async Task UpsertAsync(UpsertTournamentPlayerInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        _ = await _dbContext.Tournaments.FindAsync([command.TournamentId], cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);

        var entry = await _dbContext.TournamentPlayerInfos
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                cancellationToken);

        var addNew = entry is null;

        entry ??= new TournamentPlayerInfo();
        entry.TournamentId = command.TournamentId;
        entry.AccountId = command.AccountId;
        entry.Payload = command.Payload;

        if (addNew)
            await _dbContext.TournamentPlayerInfos.AddAsync(entry, cancellationToken);
        else
            _dbContext.TournamentPlayerInfos.Update(entry);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
