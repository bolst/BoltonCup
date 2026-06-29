using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

/// <summary>An account a ranking is currently shared with.</summary>
public record CustomRankingShareInfo(int AccountId, string Name, string Email, string? Avatar);

/// <summary>An account that can be invited to view a ranking (a GM of the ranking's tournament).</summary>
public record RankingInviteCandidate(int AccountId, string Name, string Email);

public interface ICustomRankingService
{
    Task<IReadOnlyList<CustomRanking>> GetForAccountAsync(int accountId, int? tournamentId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomRanking>> GetSharedWithAccountAsync(int accountId, int? tournamentId = null, CancellationToken cancellationToken = default);
    Task<CustomRanking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateCustomRankingCommand command, CancellationToken cancellationToken = default);
    Task<int> CloneAsync(int sourceRankingId, int ownerAccountId, string? title = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateCustomRankingCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges the tournament's current player pool into the ranking: newly-registered players are added
    /// to the "needs ranking" tray. Returns the set of player ids that are in the ranking but no longer
    /// in the pool (stale entries to flag for removal).
    /// </summary>
    Task<IReadOnlySet<int>> ReconcileAsync(int rankingId, CancellationToken cancellationToken = default);

    /// <summary>Removes a single player from a ranking (used to drop a stale entry).</summary>
    Task RemovePlayerAsync(int rankingId, int playerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomRankingShareInfo>> GetSharesAsync(int rankingId, CancellationToken cancellationToken = default);
    Task AddShareAsync(int rankingId, int accountId, CancellationToken cancellationToken = default);
    Task RemoveShareAsync(int rankingId, int accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingInviteCandidate>> SearchInvitableGmsAsync(int rankingId, string? query, int limit = 5, CancellationToken cancellationToken = default);
}
