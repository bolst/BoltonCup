using System.Text.Json;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TournamentPlayerInfoService(BoltonCupDbContext _dbContext) : ITournamentPlayerInfoService
{
    private const int MaxPayloadLength = 8192;

    public async Task<TournamentPlayerInfoContext> GetAsync(int tournamentId, int accountId,
        CancellationToken cancellationToken = default)
    {
        var info = await _dbContext.TournamentPlayerInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AccountId == accountId && e.TournamentId == tournamentId, cancellationToken);

        var player = await _dbContext.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccountId == accountId && p.TournamentId == tournamentId, cancellationToken)
            ?? throw new AccountNotInTournamentException(accountId, tournamentId);

        if (player.TeamId is not { } teamId)
        {
            return new TournamentPlayerInfoContext(info, []);
        }

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
        ValidatePayload(command.Payload);

        _ = await _dbContext.Tournaments.FindAsync([command.TournamentId], cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);

        _ = await _dbContext.Players 
                .AsNoTracking() 
                .FirstOrDefaultAsync(p => p.AccountId == command.AccountId && p.TournamentId == command.TournamentId, cancellationToken) 
            ?? throw new AccountNotInTournamentException(command.AccountId, command.TournamentId);

        var entry = await _dbContext.TournamentPlayerInfos
            .SingleOrDefaultAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                cancellationToken);

        if (entry is null)
        {
            entry = new TournamentPlayerInfo
            {
                TournamentId = command.TournamentId,
                AccountId = command.AccountId,
                Payload = command.Payload,
            };
            await _dbContext.TournamentPlayerInfos.AddAsync(entry, cancellationToken);
        }
        else
        {
            entry.Payload = command.Payload;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) when (_dbContext.Entry(entry).State == EntityState.Added)
        {
            // Lost an insert race against a concurrent first-time upsert for the same
            // (account, tournament); the unique index rejected our INSERT. Reload the
            // row the winner created and apply our payload as an update instead.
            _dbContext.Entry(entry).State = EntityState.Detached;

            var existing = await _dbContext.TournamentPlayerInfos
                .SingleAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                    cancellationToken);
            existing.Payload = command.Payload;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static void ValidatePayload(string? payload)
    {
        if (payload is null)
        {
            return;
        }

        if (payload.Length > MaxPayloadLength)
        {
            throw new InvalidPlayerInfoPayloadException($"Payload exceeds the maximum length of {MaxPayloadLength} characters.");
        }

        try
        {
            using var _ = JsonDocument.Parse(payload);
        }
        catch (JsonException)
        {
            throw new InvalidPlayerInfoPayloadException("Payload is not valid JSON.");
        }
    }
}
