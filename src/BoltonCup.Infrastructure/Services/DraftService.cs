using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Infrastructure.Services;

public class DraftService(
    BoltonCupDbContext _dbContext,
    ILogger<DraftService> _logger
) : IDraftService
{
    public async Task<IPagedList<Draft>> GetAsync(GetDraftsQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Drafts
            .Include(d => d.Tournament)
            .ConditionalWhere(d => d.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(d => d.Status == query.Status, query.Status.HasValue)
            .OrderByDescending(d => d.CreatedAt)
            .ToPagedListAsync(query, cancellationToken);
    }

    public async Task<Draft?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Drafts
            .Include(d => d.Tournament)
            .Include(d => d.DraftOrders)
                .ThenInclude(d => d.Team)
            .Include(d => d.DraftPicks)
                .ThenInclude(d => d.Team)
            .Include(d => d.DraftPicks)
                .ThenInclude(d => d.Player)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public Task CreateAsync(CreateDraftCommand command, CancellationToken cancellationToken = default)
    {
        _dbContext.Drafts.Add(new Draft
        {
            TournamentId = command.TournamentId,
            Status = DraftStatus.Pending,
        });
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateDraftCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
            .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);


        var regeneratePicks = await EnsureDraftStatusIsValidAsync(draft, command.DraftStatus, cancellationToken);
        draft.Status = command.DraftStatus;
        
        // draft type can only be changed in the pending state
        if (command.DraftType != draft.Type && draft.Status != DraftStatus.Pending)
            throw new InvalidOperationException("Draft type cannot change once the draft has started.");
        draft.Type = command.DraftType;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        if (regeneratePicks)
        {
            await RegenerateDraftPicksAsync(draft, cancellationToken);
        }
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Drafts
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
    
    
    public async Task<DraftPick?> GetCurrentPickAsync(int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts.SingleOrDefaultAsync(d => d.Id == draftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);
        if (draft.Status != DraftStatus.InProgress)
            throw new InvalidOperationException("Draft is not in progress.");

        return await _dbContext.DraftPicks
            .Where(dp => dp.DraftId == draft.Id)
            .Where(dp => dp.PlayerId == null)
            .OrderBy(dp => dp.OverallPick)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task DraftPlayerAsync(DraftPlayerCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .Include(d => d.DraftPicks)
                        .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);
        
        if (draft.Status != DraftStatus.InProgress)
            throw new InvalidOperationException("Draft is not in progress.");
        
        // make sure player is in draft's tournament
        var isValidPlayer = await _dbContext.Players
            .Where(p => p.TournamentId == draft.TournamentId)
            .AnyAsync(p => p.Id == command.PlayerId, cancellationToken);
        if (!isValidPlayer) 
            throw new EntityNotFoundException(nameof(Player), command.PlayerId);
        // make sure player is not already drafted
        if (draft.DraftPicks.Any(dp => dp.PlayerId == command.PlayerId))
            throw new InvalidOperationException($"Player {command.PlayerId} is already drafted");
        // make sure pick exists and matches command
        var pick = draft.DraftPicks
                       .Where(dp => dp.OverallPick == command.OverallPick)
                       .Where(dp => dp.TeamId == command.TeamId) 
                       .FirstOrDefault(dp => dp.PlayerId == null)
                   ?? throw new InvalidOperationException($"No available draft picks for Draft {command.DraftId}");
        
        // draft player
        pick.PlayerId = command.PlayerId;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Draft pick version expired");
        }
    }


    // status can only evolve as Pending -> InProgress or InProgress -> Completed
    // will throw on validation error
    // returns true if picks need to be regenerated, otherwise false
    private async Task<bool> EnsureDraftStatusIsValidAsync(Draft draft, DraftStatus newStatus, CancellationToken cancellationToken)
    {
        if (draft.Status == newStatus)
            return false;
        
        switch (draft.Status)
        {
            case DraftStatus.Pending:
            {
                if (newStatus != DraftStatus.InProgress)
                    throw new InvalidOperationException(
                        "Invalid state change (Pending can only evolve to In Progress)");
                    
                // if the draft is now in progress, we need to generate the picks
                return true;
            }
            case DraftStatus.InProgress:
            {
                if (newStatus != DraftStatus.Completed)
                    throw new InvalidOperationException(
                        "Invalid state change (In Progress can only evolve to Completed");

                var arePlayersUndrafted = await _dbContext.DraftPicks.AnyAsync(
                    dp => dp.DraftId == draft.Id && dp.PlayerId == null, cancellationToken: cancellationToken);
                if (arePlayersUndrafted)
                    throw new InvalidOperationException("Draft still has players who need to be drafted");
                
                break;
            }
            case DraftStatus.Completed:
            default:
                throw new InvalidOperationException("Cannot modify the state of an already-completed draft.");
        }

        return false;
    }
    
    
    private async Task RegenerateDraftPicksAsync(Draft draft, CancellationToken cancellationToken)
    {
        // delete old picks
        await _dbContext.DraftPicks
            .Where(d => d.DraftId == draft.Id)
            .ExecuteDeleteAsync(cancellationToken);
        
        var draftOrders = await _dbContext.DraftOrders
            .Where(d => d.DraftId == draft.Id)
            .OrderBy(d => d.Pick)
            .ToDictionaryAsync(d => d.Pick, cancellationToken);
        var teamCount = draftOrders.Count;
        if (teamCount == 0)
            throw new InvalidOperationException($"Draft {draft.Id} has no teams/orders");
        
        var totalPlayerCount = await _dbContext.Players
            .Where(p => p.TeamId == null)
            .Where(p => p.TournamentId == draft.TournamentId)
            .CountAsync(cancellationToken);
        
        for (int i = 0; i < totalPlayerCount; i++)
        {
            var round = i / teamCount + 1;
            var standardRoundPick = i % teamCount + 1;
            var roundPick = standardRoundPick;
            
            // if draft is snake, we reverse the order in even rounds
            if (draft.Type == DraftType.Snake && round % 2 == 0)
                roundPick = teamCount - standardRoundPick + 1;

            if (draftOrders.GetValueOrDefault(roundPick)?.TeamId is not { } teamId) 
                throw new InvalidOperationException($"Cannot generate picks: Draft {draft.Id} has an invalid ordering.");
            
            _dbContext.DraftPicks.Add(new DraftPick
            {
                DraftId = draft.Id,
                OverallPick = i + 1,
                TeamId = teamId,
                PlayerId = null
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}