using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Infrastructure.Services;

public class DraftService(
    BoltonCupDbContext _dbContext
) : IDraftService
{
    public async Task<IPagedList<Draft>> GetAsync(GetDraftsQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Drafts
            .Include(d => d.Tournament)
            .ConditionalWhere(d => d.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(d => d.Status == query.Status, query.Status.HasValue)
            // non-admins can only see visible drafts or drafts they own
            .ConditionalWhere(
                d => d.IsVisible || (d.DraftOwnerAccountId != null && d.DraftOwnerAccountId == query.AccountId),
                !query.IsAdmin
            )
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
                .ThenInclude(p => p!.Account)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<int> CreateAsync(CreateDraftCommand command, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Tournaments.AllAsync(t => t.Id != command.TournamentId, cancellationToken))
            throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // create draft
        var newDraft = new Draft
        {
            TournamentId = command.TournamentId,
            Title = command.Title,
            Status = DraftStatus.Pending,
            IsVisible = false,
            DraftOwnerAccountId = command.OwnerAccountId,
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

        await GenerateDraftRankingsAsync(newDraft, cancellationToken);
        await GenerateDraftPicksAsync(newDraft, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        
        return newDraft.Id;
    }
    
    
    public async Task<CurrentDraftState> UpdateAsync(int draftId, UpdateDraftCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .Include(draft => draft.Tournament)
                        .Include(draft => draft.DraftOrders)
                        .Include(draft => draft.DraftPicks)
                        .ThenInclude(dp => dp.Team)
                        .Include(draft => draft.DraftPicks)
                        .ThenInclude(dp => dp.Player)
                        .ThenInclude(p => p!.Account)
                        .SingleOrDefaultAsync(e => e.Id == draftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        // regenerate draft picks if draft type changes or ordering changes
        var regeneratePicks = (command.DraftType.HasValue && command.DraftType != draft.Type) || command.Ordering is not null;
        if (regeneratePicks && draft.Status != DraftStatus.Pending)
        {
            throw new InvalidOperationException("Draft type/ordering cannot be changed once draft has started.");
        }
        
        if (command.DraftType.HasValue)
        {
            draft.Type = command.DraftType.Value;
        }
        if (!string.IsNullOrEmpty(command.Title))
        {
            draft.Title = command.Title;
        }
        if (command.IsVisible.HasValue)
        {
            draft.IsVisible = command.IsVisible.Value;
        }
        if (command.SecondsPerPick.HasValue)
        {
            draft.SecondsPerPick = command.SecondsPerPick.Value;
        }
        if (command.Ordering is not null)
        {
            if (!command.Ordering.All(x => draft.DraftOrders.Any(d => d.TeamId == x.TeamId)))
            {
                throw new InvalidOperationException($"Invalid teams for draft {draft.Id}");
            }
            if (draft.DraftOrders.Count != command.Ordering.Count ||
                command.Ordering.Count != command.Ordering.Distinct().Count())
            {
                throw new InvalidOperationException("Supplied orderings are invalid (non-distinct picks or missing/extra teams).");
            }
            
            var existingAutoPick = draft.DraftOrders.ToDictionary(d => d.TeamId, d => d.AutoPick);
            draft.DraftOrders = command.Ordering
                .Select(d => new DraftOrder
                {
                    DraftId = draftId,
                    TeamId = d.TeamId,
                    Pick = d.Pick,
                    AutoPick = existingAutoPick.GetValueOrDefault(d.TeamId),
                })
                .ToList();
        }
        if (command.AutoPickSettings is not null)
        {
            foreach (var setting in command.AutoPickSettings)
            {
                var order = draft.DraftOrders.FirstOrDefault(d => d.TeamId == setting.TeamId);
                order?.AutoPick = setting.AutoPick;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (regeneratePicks)
        {
            await GenerateDraftPicksAsync(draft, cancellationToken);
        }
        
        var nextPick = await _dbContext.DraftPicks
            .Include(dp => dp.Team)
            .Include(dp => dp.Player)
            .ThenInclude(p => p!.Account)
            .Where(dp => dp.DraftId == draft.Id)
            .Where(dp => dp.PlayerId == null)
            .OrderBy(dp => dp.OverallPick)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
        
        return new CurrentDraftState(
            Draft: draft,
            NextPick: nextPick
        );
    }

    public async Task<CurrentDraftState> ApplyDefaultRankingAsync(int draftId, int? rankingId, int callerAccountId,
        bool isAdmin, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .Include(d => d.Tournament)
                        .Include(d => d.DraftOrders)
                        .ThenInclude(o => o.Team)
                        .Include(d => d.DraftPicks)
                        .ThenInclude(dp => dp.Team)
                        .Include(d => d.DraftPicks)
                        .ThenInclude(dp => dp.Player)
                        .ThenInclude(p => p!.Account)
                        .SingleOrDefaultAsync(d => d.Id == draftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var rankings = await _dbContext.PlayerDraftRankings
            .Where(r => r.DraftId == draftId)
            .ToListAsync(cancellationToken);

        if (rankingId is null)
        {
            ResetDraftRankingsToDefault(rankings);
            draft.DefaultCustomRankingId = null;
        }
        else
        {
            var ranking = await _dbContext.CustomRankings
                              .Include(r => r.Players)
                              .FirstOrDefaultAsync(r => r.Id == rankingId.Value, cancellationToken)
                          ?? throw new EntityNotFoundException(nameof(CustomRanking), rankingId.Value);

            if (!isAdmin && ranking.AccountId != callerAccountId)
                throw new InvalidOperationException("Cannot apply a custom ranking you do not own.");

            if (ranking.TournamentId != draft.TournamentId)
                throw new InvalidOperationException("Custom ranking belongs to a different tournament than the draft.");

            var rankByPlayerId = ranking.Players.ToDictionary(p => p.PlayerId, p => p.Rank);
            foreach (var row in rankings)
            {
                row.DraftRanking = rankByPlayerId.TryGetValue(row.PlayerId, out var rank) ? rank : int.MaxValue;
            }

            draft.DefaultCustomRankingId = rankingId.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var nextPick = await _dbContext.DraftPicks
            .Include(dp => dp.Team)
            .Include(dp => dp.Player)
            .ThenInclude(p => p!.Account)
            .Where(dp => dp.DraftId == draft.Id)
            .Where(dp => dp.PlayerId == null)
            .OrderBy(dp => dp.OverallPick)
            .FirstOrDefaultAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new CurrentDraftState(Draft: draft, NextPick: nextPick);
    }

    public async Task<CurrentDraftState> SetPlayerPoolAsync(int draftId, SetPlayerPoolCommand command,
        CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .Include(d => d.Tournament)
                        .Include(d => d.DraftOrders)
                        .ThenInclude(o => o.Team)
                        .Include(d => d.DraftPicks)
                        .ThenInclude(dp => dp.Team)
                        .Include(d => d.DraftPicks)
                        .ThenInclude(dp => dp.Player)
                        .ThenInclude(p => p!.Account)
                        .SingleOrDefaultAsync(d => d.Id == draftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);

        if (draft.Status != DraftStatus.Pending)
            throw new InvalidOperationException("Player pool can only be changed before the draft starts.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var rankings = await _dbContext.PlayerDraftRankings
            .Where(r => r.DraftId == draftId)
            .ToListAsync(cancellationToken);

        var rankingByPlayerId = rankings.ToDictionary(r => r.PlayerId);

        var excluded = command.ExcludedPlayerIds.ToHashSet();

        // The supplied ordered + excluded sets must exactly cover the draft's players.
        var supplied = command.OrderedPlayerIds.Concat(command.ExcludedPlayerIds).ToHashSet();
        if (supplied.Count != rankings.Count || !rankings.All(r => supplied.Contains(r.PlayerId)))
            throw new InvalidOperationException("Supplied player pool does not match the draft's players.");

        for (var i = 0; i < command.OrderedPlayerIds.Count; i++)
        {
            if (!rankingByPlayerId.TryGetValue(command.OrderedPlayerIds[i], out var row))
                throw new InvalidOperationException($"Unknown player {command.OrderedPlayerIds[i]} for draft {draftId}.");
            row.DraftRanking = i + 1;
            row.IsExcluded = false;
        }

        foreach (var playerId in excluded)
        {
            if (rankingByPlayerId.TryGetValue(playerId, out var row))
                row.IsExcluded = true;
        }

        // Manual curation supersedes an applied template.
        draft.DefaultCustomRankingId = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await GenerateDraftPicksAsync(draft, cancellationToken);

        var nextPick = await _dbContext.DraftPicks
            .Include(dp => dp.Team)
            .Include(dp => dp.Player)
            .ThenInclude(p => p!.Account)
            .Where(dp => dp.DraftId == draft.Id)
            .Where(dp => dp.PlayerId == null)
            .OrderBy(dp => dp.OverallPick)
            .FirstOrDefaultAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new CurrentDraftState(Draft: draft, NextPick: nextPick);
    }

    private static void ResetDraftRankingsToDefault(List<PlayerDraftRanking> rankings)
    {
        var ordered = rankings
            .OrderByDescending(r => r.PointsPerGame)
            .ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].DraftRanking = i + 1;
        }
    }

    public async Task StartAsync(int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts 
                        .SingleOrDefaultAsync(e => e.Id == draftId, cancellationToken) 
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);
        
        if (draft.Status == DraftStatus.Completed)
            throw new InvalidOperationException($"Draft {draft.Id} is completed and cannot be started.");

        draft.Status = DraftStatus.InProgress;

        // Start (or, on resume, reset) the clock on the pick currently on the clock.
        var currentPick = await _dbContext.DraftPicks
            .Where(dp => dp.DraftId == draft.Id)
            .Where(dp => dp.PlayerId == null)
            .OrderBy(dp => dp.OverallPick)
            .FirstOrDefaultAsync(cancellationToken);
        currentPick?.ClockStartedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task PauseAsync(int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts 
                        .SingleOrDefaultAsync(e => e.Id == draftId, cancellationToken) 
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);
        
        if (draft.Status is DraftStatus.Completed or DraftStatus.Pending)
            throw new InvalidOperationException($"Draft {draft.Id} is completed and cannot be paused.");
        
        draft.Status = DraftStatus.Paused;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task EndAsync(int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts 
                        .SingleOrDefaultAsync(e => e.Id == draftId, cancellationToken) 
                    ?? throw new EntityNotFoundException(nameof(Draft), draftId);
        
        draft.Status = DraftStatus.Completed;
        await _dbContext.SaveChangesAsync(cancellationToken);
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
        if (draft.Status == DraftStatus.Completed)
            throw new InvalidOperationException("Draft is completed.");

        return await _dbContext.DraftPicks
            .Include(dp => dp.Team)
            .Include(dp => dp.Player)
                .ThenInclude(p => p!.Account)
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
            .Include(d => d.DraftPick)
                .ThenInclude(dp => dp!.Team)
            .Include(d => d.Tournament)
            .Where(d => d.DraftId == draftId)
            .ConditionalWhere(d => d.Player.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .ConditionalWhere(d => d.DraftPick != null && d.DraftPick.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(d => (d.DraftPickId != null) == query.IsDrafted!.Value, query.IsDrafted.HasValue)
            .ApplySorting(query, x => x.OrderBy(y => y.DraftRanking))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlySet<int>> GetFavouritePlayerIdsAsync(int draftId, int accountId,
        CancellationToken cancellationToken = default)
    {
        var playerIds = await _dbContext.PlayerFavourites
            .AsNoTracking()
            .Where(f => f.DraftId == draftId && f.AccountId == accountId)
            .Select(f => f.PlayerId)
            .ToListAsync(cancellationToken);

        return playerIds.ToHashSet();
    }

    public async Task<bool> ToggleFavouriteAsync(int draftId, int playerId, int accountId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.PlayerFavourites
            .SingleOrDefaultAsync(
                f => f.DraftId == draftId && f.AccountId == accountId && f.PlayerId == playerId,
                cancellationToken);

        if (existing is not null)
        {
            _dbContext.PlayerFavourites.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        _dbContext.PlayerFavourites.Add(new PlayerFavourite
        {
            DraftId = draftId,
            AccountId = accountId,
            PlayerId = playerId,
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }


    public async Task<CurrentDraftStateWithPick> DraftPlayerAsync(DraftPlayerCommand command, CancellationToken cancellationToken = default)
    {
        var draft = await _dbContext.Drafts
                        .Include(d => d.DraftPicks)
                        .ThenInclude(dp => dp.Team)
                        .Include(d => d.DraftPicks)
                        .ThenInclude(dp => dp.Player)
                        .ThenInclude(p => p!.Account)
                        .SingleOrDefaultAsync(e => e.Id == command.DraftId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(Draft), command.DraftId);
        
        if (draft.Status != DraftStatus.InProgress)
            throw new InvalidOperationException("Draft is not in progress.");

        var player = await _dbContext.PlayerDraftRankings
            .Where(p => p.DraftId == command.DraftId)
            .Include(p => p.Player)
            .FirstOrDefaultAsync(p => p.PlayerId == command.PlayerId, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PlayerDraftRanking), command.PlayerId);

        if (player.IsDrafted)
        {
            throw new InvalidOperationException($"Player {command.PlayerId} is already drafted");
        }

        if (player.IsExcluded)
        {
            throw new InvalidOperationException($"Player {command.PlayerId} is excluded from this draft");
        }

        // make sure pick exists and matches command
        var pick = draft.DraftPicks
                       .Where(dp => dp.OverallPick == command.OverallPick)
                       .Where(dp => dp.TeamId == command.TeamId)
                       .FirstOrDefault(dp => dp.PlayerId == null)
                   ?? throw new InvalidOperationException($"No available draft pick for player {command.PlayerId} in Draft {command.DraftId}");

        await EnforceGoalieRulesAsync(draft, command.TeamId, player.Player.Position, cancellationToken);

        // draft player
        pick.PlayerId = command.PlayerId;
        player.DraftPickId = pick.Id;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Draft pick version expired");
        }

        DraftPick? nextPick = null;
        if (draft.Status != DraftStatus.Completed)
        {
            nextPick = await _dbContext.DraftPicks
                .Include(dp => dp.Team)
                .Include(dp => dp.Player)
                .ThenInclude(p => p!.Account)
                .Where(dp => dp.DraftId == draft.Id)
                .Where(dp => dp.PlayerId == null)
                .OrderBy(dp => dp.OverallPick)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (nextPick is not null)
            {
                nextPick.ClockStartedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await _dbContext.Entry(pick)
            .Reference(p => p.Player)
            .Query()
            .Include(p => p.Account)
            .LoadAsync(cancellationToken);
        
        return new CurrentDraftStateWithPick(
            Draft: draft,
            CompletedPick: pick,
            NextPick: nextPick
        );
    }

    public async Task<IReadOnlyList<CurrentDraftStateWithPick>> ResolveAutoPicksAsync(int draftId, CancellationToken cancellationToken = default)
    {
        var results = new List<CurrentDraftStateWithPick>();

        while (true)
        {
            var draft = await _dbContext.Drafts
                .SingleOrDefaultAsync(d => d.Id == draftId, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Draft), draftId);
            if (draft.Status != DraftStatus.InProgress)
                break;

            var currentPick = await _dbContext.DraftPicks
                .Where(dp => dp.DraftId == draftId)
                .Where(dp => dp.PlayerId == null)
                .OrderBy(dp => dp.OverallPick)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentPick is null)
                break;

            var order = await _dbContext.DraftOrders
                .FirstOrDefaultAsync(o => o.DraftId == draftId && o.TeamId == currentPick.TeamId, cancellationToken);
            if (order is null || !order.AutoPick)
                break;

            var best = await GetBestAvailablePlayerAsync(draftId, currentPick.TeamId, cancellationToken);
            if (best is null)
                break;

            var command = new DraftPlayerCommand(draftId, best.PlayerId, currentPick.TeamId, currentPick.OverallPick);
            results.Add(await DraftPlayerAsync(command, cancellationToken));
        }

        return results;
    }

    private async Task<PlayerDraftRanking?> GetBestAvailablePlayerAsync(int draftId, int teamId, CancellationToken cancellationToken)
    {
        var available = await _dbContext.PlayerDraftRankings
            .Where(p => p.DraftId == draftId)
            .Where(p => p.DraftPickId == null)
            .Where(p => !p.IsExcluded)
            .Include(p => p.Player)
            .ToListAsync(cancellationToken);
        if (available.Count == 0)
            return null;

        var roster = await _dbContext.DraftPicks
            .Where(dp => dp.DraftId == draftId && dp.TeamId == teamId && dp.PlayerId != null)
            .Select(dp => new RosteredPlayer(dp.Player!.Position, dp.Player.CanPlayEitherPosition))
            .ToListAsync(cancellationToken);

        var remainingPicks = await _dbContext.DraftPicks
            .CountAsync(dp => dp.DraftId == draftId && dp.TeamId == teamId && dp.PlayerId == null, cancellationToken);

        var candidates = available
            .Select(p => new AutoPickCandidate(p.PlayerId, p.DraftRanking, p.Player.Position, p.Player.CanPlayEitherPosition))
            .ToList();

        var chosen = SmartAutoPickSelector.Select(candidates, roster, remainingPicks);
        if (chosen is null)
            return null;

        return available.First(p => p.PlayerId == chosen.Value.PlayerId);
    }

    private async Task EnforceGoalieRulesAsync(Draft draft, int teamId, string? position, CancellationToken cancellationToken)
    {
        var teamPicks = draft.DraftPicks.Where(dp => dp.TeamId == teamId).ToList();
        var teamGoalies = teamPicks.Count(dp =>
            dp.PlayerId != null &&
            string.Equals(dp.Player?.Position, Position.Goalie, StringComparison.OrdinalIgnoreCase));
        var teamRemaining = teamPicks.Count(dp => dp.PlayerId == null);

        var draftingGoalie = string.Equals(position, Position.Goalie, StringComparison.OrdinalIgnoreCase);

        if (draftingGoalie && teamGoalies >= 1)
            throw new InvalidOperationException("Team already has a goalie.");

        if (!draftingGoalie && teamGoalies == 0 && teamRemaining == 1)
        {
            var goalieAvailable = await _dbContext.PlayerDraftRankings
                .Where(p => p.DraftId == draft.Id && p.DraftPickId == null)
                .AnyAsync(p => p.Player.Position == Position.Goalie, cancellationToken);
            if (goalieAvailable)
                throw new InvalidOperationException("Team must use its final pick on a goalie.");
        }
    }


    private async Task GenerateDraftPicksAsync(Draft draft, CancellationToken cancellationToken)
    {
        // delete old picks
        draft.DraftPicks.Clear();
        
        var draftOrders = await _dbContext.DraftOrders
            .Where(d => d.DraftId == draft.Id)
            .OrderBy(d => d.Pick)
            .ToDictionaryAsync(d => d.Pick, cancellationToken);
        var teamCount = draftOrders.Count;
        if (teamCount == 0)
            throw new InvalidOperationException($"Draft {draft.Id} has no teams/orders");
        
        var totalPlayerCount = await _dbContext.PlayerDraftRankings
            .Where(r => r.DraftId == draft.Id && !r.IsExcluded)
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
                Round = round,
                RoundPick = roundPick,
                TeamId = teamId,
                PlayerId = null
            });
        }

        draft.Teams = teamCount;
        draft.Rounds = (int)Math.Ceiling((double)totalPlayerCount / teamCount);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task GenerateDraftRankingsAsync(Draft draft, CancellationToken cancellationToken = default)
    {
        var players = await _dbContext.Players
            .Include(p => p.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.SkaterGameLogs)
            .Include(player => player.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.GoalieGameLogs)
            .Where(p => p.TournamentId == draft.TournamentId)
            .Where(p => !_dbContext.PlayerDraftRankings.Any(pdr => pdr.PlayerId == p.Id && pdr.DraftId == draft.Id))
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