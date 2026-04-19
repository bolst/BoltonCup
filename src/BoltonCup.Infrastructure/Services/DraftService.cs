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
    IStorageService _storageService,
    IAssetKeyGenerator _assetKeyGenerator,
    ILogger<DraftService> _logger
) : IDraftService
{
    public async Task<IPagedList<Draft>> GetAsync(GetDraftsQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Drafts
            .Include(d => d.TournamentId)
            .ConditionalWhere(d => d.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(d => d.Status == query.Status, query.Status.HasValue)
            .OrderByDescending(d => d.CreatedAt)
            .ToPagedListAsync(query, cancellationToken);
    }

    public async Task<Draft?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Drafts
            .Include(d => d.TournamentId)
            .Include(d => d.DraftOrders)
                .ThenInclude(d => d.Team)
            .Include(d => d.DraftPicks)
                .ThenInclude(d => d.Team)
            .Include(d => d.DraftPicks)
                .ThenInclude(d => d.Player)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task CreateAsync(CreateDraftCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _dbContext.Tournaments
                .SingleOrDefaultAsync(e => e.Id == command.TournamentId, cancellationToken: cancellationToken) 
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);

        var draft = new Draft
        {
            TournamentId = command.TournamentId,
            Type = command.DraftType,
            Status = DraftStatus.Pending,
        };
        
        _dbContext.Drafts.Add(draft);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateDraftCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
            .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);
        
        draft.Status = command.DraftStatus;
        draft.Type = command.DraftType;
        
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Draft), id);
        
        _dbContext.Drafts.Remove(draft);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DraftPlayerAsync(DraftPlayerCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);
        
        // make sure team exists and is in draft
        var team = await _dbContext.Teams
                       .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == command.TeamId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Team), command.TeamId);
        if (team.TournamentId != draft.TournamentId)
            throw new InvalidOperationException($"Team {command.TeamId} does not belong to {draft.TournamentId}");
        
        // make sure player exists and is in draft
        var player = await _dbContext.Players
                         .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == command.PlayerId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Player), command.PlayerId);
        if (player.TournamentId != draft.TournamentId)
            throw new InvalidOperationException($"Player {command.PlayerId} does not belong to {draft.TournamentId}");

        // make sure pick exists and matches command
        var pick = await _dbContext.DraftPicks
                       .FirstOrDefaultAsync(dp => dp.DraftId == command.DraftId && dp.PlayerId == null, cancellationToken: cancellationToken)
                   ?? throw new InvalidOperationException($"No available draft picks for Draft {command.DraftId}");
        if (pick.TeamId != command.TeamId)
            throw new InvalidOperationException($"The current pick does not belong to Team {command.TeamId}");
        
        // draft player
        pick.PlayerId = command.PlayerId;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}