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

    public async Task<int> CreateAsync(CreateDraftCommand command, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Tournaments.AllAsync(t => t.Id != command.TournamentId, cancellationToken))
            throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);
        
        // create draft
        var newDraft = new Draft
        {
            TournamentId = command.TournamentId,
            Title = command.Title,
            Status = DraftStatus.Pending,
        };
        _dbContext.Drafts.Add(newDraft);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // create default orderings
        var orderings = await _dbContext.Teams
            .Where(t => t.TournamentId == command.TournamentId)
            .Select(team => new DraftOrder
            {
                DraftId = newDraft.Id,
                TeamId = team.Id,
                Pick = 0,
            })
            .ToListAsync(cancellationToken);

        orderings = orderings.Select((order, index) =>
        {
            order.Pick = index + 1;
            return order;
        }).ToList();
        
        _dbContext.DraftOrders.AddRange(orderings);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return newDraft.Id;
    }

    public async Task UpdateAsync(UpdateDraftCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
            .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);

        await EnsureDraftStatusIsValidAsync(draft, command.DraftStatus, cancellationToken);
        draft.Status = command.DraftStatus;
        
        // draft type can only be changed in the pending state
        if (command.DraftType != draft.Type && draft.Status != DraftStatus.Pending)
            throw new InvalidOperationException("Draft type cannot change once the draft has started.");
        draft.Type = command.DraftType;
        
        // draft title can only be changed in the pending state
        if (command.Title != draft.Title && draft.Status != DraftStatus.Pending)
            throw new InvalidOperationException("Draft name cannot change once the draft has started.");
        draft.Title = command.Title;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // if the draft is now in progress, we need to generate the picks
        if (draft.Status == DraftStatus.InProgress)
        {
            await GenerateDraftPicksAsync(draft, cancellationToken);
            await GenerateDraftRankingsAsync(draft, cancellationToken);
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


    public async Task<IPagedList<PlayerDraftRanking>> GetDraftRankingsAsync(int draftId, GetDraftRankingsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlayerDraftRankings
            .AsNoTracking()
            .Include(d => d.Player)
                .ThenInclude(p => p.Account)
            .Include(d => d.Tournament)
            .Where(d => d.DraftId == draftId)
            .ApplySorting(query, x => x.OrderByDescending(y => y.DraftRanking))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }
    

    public async Task UpdateOrderingAsync(UpdateDraftOrderingCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts 
                        .Include(d => d.DraftOrders) 
                        .FirstOrDefaultAsync(d => d.Id == command.DraftId, cancellationToken) 
                    ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);

        if (draft.Status != DraftStatus.Pending)
            throw new InvalidOperationException("Cannot update draft ordering once draft starts.");

        if (!command.Ordering.All(x => draft.DraftOrders.Any(d => d.TeamId == x.TeamId)))
            throw new InvalidOperationException($"Invalid teams for Draft {draft.Id}.");
        
        if (command.Ordering.Any(x => x.Pick < 1 || x.Pick > command.Ordering.Count))
            throw new InvalidOperationException($"Invalid picks.");
        
        draft.DraftOrders = command.Ordering
            .Select(d => new DraftOrder
            {
                DraftId = command.DraftId,
                TeamId = d.TeamId,
                Pick = d.Pick,
            })
            .ToList();
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    

    public async Task DraftPlayerAsync(DraftPlayerCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .Include(d => d.DraftPicks)
                        .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);
        
        if (draft.Status != DraftStatus.InProgress)
            throw new InvalidOperationException("Draft is not in progress.");

        var player = await _dbContext.PlayerDraftRankings
            .Where(p => p.DraftId == command.DraftId)
            .FirstOrDefaultAsync(p => p.PlayerId == command.PlayerId, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PlayerDraftRanking), command.PlayerId);

        if (player.IsDrafted)
        {
            throw new InvalidOperationException($"Player {command.PlayerId} is already drafted");
        }
        
        // make sure pick exists and matches command
        var pick = draft.DraftPicks
                       .Where(dp => dp.OverallPick == command.OverallPick)
                       .Where(dp => dp.TeamId == command.TeamId) 
                       .FirstOrDefault(dp => dp.PlayerId == null)
                   ?? throw new InvalidOperationException($"No available draft pick for player {command.PlayerId} in Draft {command.DraftId}");
        
        // draft player
        pick.PlayerId = command.PlayerId;
        player.IsDrafted = true;

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
    private async Task EnsureDraftStatusIsValidAsync(Draft draft, DraftStatus newStatus, CancellationToken cancellationToken)
    {
        if (draft.Status == newStatus)
            return;
        
        switch (draft.Status)
        {
            case DraftStatus.Pending:
            {
                if (newStatus != DraftStatus.InProgress)
                    throw new InvalidOperationException(
                        "Invalid state change (Pending can only evolve to In Progress)");
                break;
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
    }
    
    
    private async Task GenerateDraftPicksAsync(Draft draft, CancellationToken cancellationToken)
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
    
    public async Task GenerateDraftRankingsAsync(Draft draft, CancellationToken cancellationToken = default)
    {
        var players = await _dbContext.Players
            .Include(p => p.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.SkaterGameLogs)
            .Include(player => player.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.GoalieGameLogs)
            .Where(p => p.TournamentId == draft.TournamentId)
            .Where(p => _dbContext.PlayerDraftRankings.All(pdr => pdr.PlayerId != p.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var rankings = players
            .Select(player =>
            {
                var skaterLogs = player.Account.Players
                    .SelectMany(p => p.SkaterGameLogs)
                    .ToList();
                var goalieLogs = player.Account.Players
                    .SelectMany(p => p.GoalieGameLogs)
                    .ToList();

                var totalPoints = skaterLogs.Sum(x => x.Points);
                return new PlayerDraftRanking
                {
                    PlayerId = player.Id,
                    TournamentId = draft.TournamentId,
                    DraftId = draft.Id,
                    GamesPlayed = skaterLogs.Count + goalieLogs.Count,
                    TotalPoints = totalPoints,
                    IsChampion = false, // TODO
                    DraftRanking = 0,
                    OverrideRanking = false,
                };
            })
            .OrderByDescending(r => r.PointsPerGame)
            .Select((player, index) =>
            {
                player.DraftRanking = index + 1;
                return player;
            });

        _dbContext.PlayerDraftRankings.AddRange(rankings);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}