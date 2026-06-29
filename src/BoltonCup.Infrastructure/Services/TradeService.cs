using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TradeService(
    BoltonCupDbContext _dbContext,
    IRosterValidator _rosterValidator,
    IEmailer _emailer,
    ISmsSender _smsSender,
    UserManager<BoltonCupUser> _userManager
) : ITradeService
{
    private const string SiteBaseUrl = "https://boltoncup.ca";

    public async Task<IReadOnlyList<Trade>> GetByTournamentAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trades
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.ProposingTeam)
            .Include(t => t.ReceivingTeam)
            .Include(t => t.Players)
            .ThenInclude(tp => tp.Player)
            .ThenInclude(p => p.Account)
            .Where(t => t.TournamentId == tournamentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Trade?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trades
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.ProposingTeam)
            .Include(t => t.ReceivingTeam)
            .Include(t => t.Players)
            .ThenInclude(tp => tp.Player)
            .ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
    
    public async Task<int> CreateAsync(CreateTradeCommand command, CancellationToken cancellationToken = default)
    {
        var tournament = await _dbContext.Tournaments.FirstOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);
        if (!tournament.IsTradingOpen)
        {
            throw new TradingClosedException(command.TournamentId);
        }
        if (command.ProposingTeamId == command.ReceivingTeamId)
        {
            throw new InvalidTradeStateException("A team cannot trade with itself.");
        }

        var proposingTeam = await _dbContext.Teams
                                .Include(t => t.GeneralManagers)
                                .FirstOrDefaultAsync(t => t.Id == command.ProposingTeamId, cancellationToken)
                            ?? throw new EntityNotFoundException(nameof(Team), command.ProposingTeamId);
        var receivingTeam = await _dbContext.Teams
                                .Include(t => t.GeneralManagers)
                                .FirstOrDefaultAsync(t => t.Id == command.ReceivingTeamId, cancellationToken)
                            ?? throw new EntityNotFoundException(nameof(Team), command.ReceivingTeamId);

        if (proposingTeam.TournamentId != command.TournamentId || receivingTeam.TournamentId != command.TournamentId)
        {
            throw new InvalidTradeStateException("Both teams must belong to the same tournament.");
        }

        var proposingIds = command.ProposingPlayerIds.Distinct().ToList();
        var receivingIds = command.ReceivingPlayerIds.Distinct().ToList();
        if (proposingIds.Count == 0 && receivingIds.Count == 0)
        {
            throw new InvalidTradeStateException("A trade must include at least one player.");
        }
        if (proposingIds.Intersect(receivingIds).Any())
        {
            throw new InvalidTradeStateException("A player cannot be on both sides of a trade.");
        }

        var allIds = proposingIds.Concat(receivingIds).ToList();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var playerMap = await _dbContext.Players
            .Include(p => p.Account)
            .Where(p => p.TournamentId == command.TournamentId)
            .Where(p => allIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var gmAccountIds = await GetGmAccountIdsAsync(command.TournamentId, cancellationToken);

        ValidateSide(proposingIds, command.ProposingTeamId, playerMap, gmAccountIds);
        ValidateSide(receivingIds, command.ReceivingTeamId, playerMap, gmAccountIds);
        await ValidateLockedPlayersAsync(allIds, cancellationToken);

        var trade = new Trade
        {
            TournamentId = command.TournamentId,
            ProposingTeamId = command.ProposingTeamId,
            ReceivingTeamId = command.ReceivingTeamId,
            Status = TradeStatus.Pending,
            Note = command.Note,
            CreatedByAccountId = command.CreatedByAccountId,
        };
        foreach (var id in proposingIds)
        {
            trade.Players.Add(new TradePlayer
            {
                PlayerId = id,
                FromTeamId = command.ProposingTeamId,
                ToTeamId = command.ReceivingTeamId,
                Player = playerMap[id],
                IsLocked = true,
            });
        }
        foreach (var id in receivingIds)
        {
            trade.Players.Add(new TradePlayer
            {
                PlayerId = id,
                FromTeamId = command.ReceivingTeamId,
                ToTeamId = command.ProposingTeamId,
                Player = playerMap[id],
            });
        }

        _dbContext.Trades.Add(trade);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        trade.ProposingTeam = proposingTeam;
        trade.ReceivingTeam = receivingTeam;
        var info = BuildEmailInfo(trade);
        var recipients = await GetRecipientsAsync(trade, includePlayers: false);
        await _emailer.SendTradeCreatedAsync(recipients, info);
        await SendSmsAsync(trade.ReceivingTeam.GeneralManagers, BuildTradeProposalSms(trade.TournamentId, info));

        return trade.Id;
    }

    private async Task SendSmsAsync(IEnumerable<Account> gms, string message)
    {
        var phones = gms
            .Select(gm => gm.Phone)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();
        foreach (var phone in phones)
        {
            await _smsSender.SendAsync(phone!, message);
        }
    }

    private static string TradeHubLink(int tournamentId) =>
        $"Click the link to go to the trade hub: {SiteBaseUrl}/tournaments/{tournamentId}/trade-hub";

    private static string BuildTradeProposalSms(int tournamentId, TradeEmailInfo info)
    {
        var proposingTeam = info.ProposingTeamName.ToUpperInvariant();
        var lines = new List<string> { $"{proposingTeam} has sent you a trade.", "Your team receives:" };
        lines.AddRange(PlayerLines(info.PlayersFromProposing));
        lines.Add($"{proposingTeam} receives:");
        lines.AddRange(PlayerLines(info.PlayersFromReceiving));
        lines.Add(TradeHubLink(tournamentId));
        return string.Join("\n", lines);

        static IEnumerable<string> PlayerLines(IReadOnlyList<string> players) =>
            players.Count == 0 ? ["- none"] : players.Select(p => $"- {p}");
    }

    private static string BuildTradeAcceptedSms(Trade trade) => string.Join("\n",
        $"{trade.ReceivingTeam.Name.ToUpperInvariant()} accepted your trade. It now awaits admin approval.",
        TradeHubLink(trade.TournamentId));

    private static string BuildTradeDeclinedSms(Trade trade) => string.Join("\n",
        $"{trade.ReceivingTeam.Name.ToUpperInvariant()} declined your trade.",
        TradeHubLink(trade.TournamentId));

    private static string BuildTradeCancelledSms(Trade trade) => string.Join("\n",
        $"The trade between {trade.ProposingTeam.Name.ToUpperInvariant()} and {trade.ReceivingTeam.Name.ToUpperInvariant()} was cancelled.",
        TradeHubLink(trade.TournamentId));

    private static string BuildTradeApprovedSms(Trade trade) => string.Join("\n",
        $"The trade between {trade.ProposingTeam.Name.ToUpperInvariant()} and {trade.ReceivingTeam.Name.ToUpperInvariant()} was approved. Rosters have been updated.",
        TradeHubLink(trade.TournamentId));

    public async Task AcceptAsync(int tradeId, int accountId, CancellationToken cancellationToken = default)
    {
        var trade = await LoadTradeAsync(tradeId, cancellationToken);
        if (!trade.Tournament.IsTradingOpen)
        {
            throw new TradingClosedException(trade.TournamentId);
        }
        if (trade.Status != TradeStatus.Pending)
        {
            throw new InvalidTradeStateException($"Only pending trades can be accepted (current status: {trade.Status}).");
        }

        RevalidatePlayers(trade, await GetGmAccountIdsAsync(trade.TournamentId, cancellationToken));

        trade.Status = TradeStatus.Accepted;
        trade.RespondedByAccountId = accountId;
        trade.RespondedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailer.SendTradeAcceptedAsync(await GetRecipientsAsync(trade, includePlayers: false), BuildEmailInfo(trade));
        await SendSmsAsync(trade.ProposingTeam.GeneralManagers, BuildTradeAcceptedSms(trade));
    }

    public async Task DeclineAsync(int tradeId, int accountId, CancellationToken cancellationToken = default)
    {
        var trade = await LoadTradeAsync(tradeId, cancellationToken);
        if (trade.Status != TradeStatus.Pending)
        {
            throw new InvalidTradeStateException($"Only pending trades can be declined (current status: {trade.Status}).");
        }

        trade.Status = TradeStatus.Declined;
        trade.RespondedByAccountId = accountId;
        trade.RespondedAt = DateTime.UtcNow;
        ReleasePlayers(trade);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailer.SendTradeDeclinedAsync(await GetRecipientsAsync(trade, includePlayers: false), BuildEmailInfo(trade));
        await SendSmsAsync(trade.ProposingTeam.GeneralManagers, BuildTradeDeclinedSms(trade));
    }

    public async Task CancelAsync(int tradeId, int accountId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var trade = await LoadTradeAsync(tradeId, cancellationToken);

        if (isAdmin)
        {
            if (trade.Status is not (TradeStatus.Pending or TradeStatus.Accepted))
            {
                throw new InvalidTradeStateException($"Only pending or accepted trades can be cancelled (current status: {trade.Status}).");
            }
        }
        else if (trade.Status != TradeStatus.Pending)
        {
            throw new InvalidTradeStateException($"A proposing GM can only cancel a pending trade (current status: {trade.Status}).");
        }

        trade.Status = TradeStatus.Cancelled;
        trade.ResolvedByAccountId = accountId;
        trade.ResolvedAt = DateTime.UtcNow;
        ReleasePlayers(trade);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailer.SendTradeCancelledAsync(await GetRecipientsAsync(trade, includePlayers: false), BuildEmailInfo(trade));
        var cancelRecipients = isAdmin
            ? trade.ProposingTeam.GeneralManagers.Concat(trade.ReceivingTeam.GeneralManagers)
            : trade.ReceivingTeam.GeneralManagers;
        await SendSmsAsync(cancelRecipients, BuildTradeCancelledSms(trade));
    }

    public async Task ApproveAsync(int tradeId, int accountId, CancellationToken cancellationToken = default)
    {
        var trade = await LoadTradeAsync(tradeId, cancellationToken);
        if (trade.Status != TradeStatus.Accepted)
        {
            throw new InvalidTradeStateException($"Only accepted trades can be approved (current status: {trade.Status}).");
        }

        RevalidatePlayers(trade, await GetGmAccountIdsAsync(trade.TournamentId, cancellationToken));
        await ValidatePostTradeRostersAsync(trade, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var tp in trade.Players)
        {
            tp.Player.TeamId = tp.ToTeamId;
        }

        trade.Status = TradeStatus.Approved;
        trade.ResolvedByAccountId = accountId;
        trade.ResolvedAt = DateTime.UtcNow;
        ReleasePlayers(trade);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _emailer.SendTradeApprovedAsync(await GetRecipientsAsync(trade, includePlayers: true), BuildEmailInfo(trade));
        await SendSmsAsync(trade.ProposingTeam.GeneralManagers.Concat(trade.ReceivingTeam.GeneralManagers), BuildTradeApprovedSms(trade));
    }

    private async Task<Trade> LoadTradeAsync(int tradeId, CancellationToken cancellationToken)
    {
        return await _dbContext.Trades
            .Include(t => t.Tournament)
            .Include(t => t.ProposingTeam).ThenInclude(team => team.GeneralManagers)
            .Include(t => t.ReceivingTeam).ThenInclude(team => team.GeneralManagers)
            .Include(t => t.Players).ThenInclude(tp => tp.Player).ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(t => t.Id == tradeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Trade), tradeId);
    }

    private static void ValidateSide(IEnumerable<int> playerIds, int fromTeamId, IReadOnlyDictionary<int, Player> playerMap, IReadOnlySet<int> gmAccountIds)
    {
        foreach (var id in playerIds)
        {
            if (!playerMap.TryGetValue(id, out var player))
            {
                throw new EntityNotFoundException(nameof(Player), id);
            }
            if (player.TeamId != fromTeamId || gmAccountIds.Contains(player.AccountId))
            {
                throw new PlayerNotTradeableException(player);
            }
        }
    }

    private static void ReleasePlayers(Trade trade)
    {
        foreach (var tp in trade.Players)
        {
            tp.IsLocked = false;
        }
    }

    private static void RevalidatePlayers(Trade trade, IReadOnlySet<int> gmAccountIds)
    {
        foreach (var tp in trade.Players)
        {
            if (tp.Player.TeamId != tp.FromTeamId || gmAccountIds.Contains(tp.Player.AccountId))
            {
                throw new PlayerNotTradeableException(tp.Player);
            }
        }
    }

    private async Task ValidatePostTradeRostersAsync(Trade trade, CancellationToken cancellationToken)
    {
        var teamIds = new[] { trade.ProposingTeamId, trade.ReceivingTeamId };
        var roster = await _dbContext.Players
            .Where(p => p.TournamentId == trade.TournamentId && p.TeamId != null && teamIds.Contains(p.TeamId.Value))
            .ToListAsync(cancellationToken);

        var destinations = trade.Players.ToDictionary(tp => tp.PlayerId, tp => tp.ToTeamId);

        foreach (var teamId in teamIds)
        {
            var postRoster = roster
                .Where(p => (destinations.TryGetValue(p.Id, out var dest) ? dest : p.TeamId!.Value) == teamId)
                .ToList();
            var result = _rosterValidator.Validate(postRoster, trade.Tournament.SkaterLimit, trade.Tournament.GoalieLimit);
            if (!result.IsValid)
            {
                throw new InvalidRosterException(result.Reasons);
            }
        }
    }

    private async Task<HashSet<int>> GetGmAccountIdsAsync(int tournamentId, CancellationToken cancellationToken)
    {
        return (await _dbContext.Teams
            .Where(t => t.TournamentId == tournamentId)
            .SelectMany(t => t.GeneralManagers.Select(g => g.Id))
            .ToListAsync(cancellationToken)).ToHashSet();
    }

    private async Task ValidateLockedPlayersAsync(IReadOnlyCollection<int> playerIds, CancellationToken cancellationToken)
    {
        var lockedTp = await _dbContext.TradePlayers
            .Include(tp => tp.Player).ThenInclude(p => p.Account)
            .Where(tp => playerIds.Contains(tp.PlayerId))
            .Where(tp => tp.IsLocked)
            .FirstOrDefaultAsync(cancellationToken);
        if (lockedTp is not null)
        {
            throw new PlayerLockedException(lockedTp.Player);
        }
    }

    private async Task<List<string>> GetRecipientsAsync(Trade trade, bool includePlayers)
    {
        var emails = new List<string>();
        emails.AddRange(trade.ProposingTeam.GeneralManagers
            .Concat(trade.ReceivingTeam.GeneralManagers)
            .Select(gm => gm.Email)
            .Where(e => !string.IsNullOrWhiteSpace(e)));

        var admins = await _userManager.GetUsersInRoleAsync(BoltonCupRole.Admin);
        emails.AddRange(admins.Select(u => u.Email).Where(e => !string.IsNullOrWhiteSpace(e))!);

        if (includePlayers)
        {
            emails.AddRange(trade.Players
                .Select(tp => tp.Player.Account?.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))!);
        }

        return emails;
    }

    private static TradeEmailInfo BuildEmailInfo(Trade trade)
    {
        var fromProposing = trade.Players.Where(tp => tp.FromTeamId == trade.ProposingTeamId).Select(Name).ToList();
        var fromReceiving = trade.Players.Where(tp => tp.FromTeamId == trade.ReceivingTeamId).Select(Name).ToList();
        return new TradeEmailInfo(trade.ProposingTeam.Name, trade.ReceivingTeam.Name, fromProposing, fromReceiving);

        static string Name(TradePlayer tp) => tp.Player?.Account is { } account
            ? $"{account.FirstName} {account.LastName}"
            : $"Player {tp.PlayerId}";
    }
}
