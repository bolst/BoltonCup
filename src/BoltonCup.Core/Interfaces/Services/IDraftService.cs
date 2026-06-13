using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IDraftService
{
    Task<IPagedList<Draft>> GetAsync(GetDraftsQuery query, CancellationToken cancellationToken = default);
    Task<Draft?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateDraftCommand command, CancellationToken cancellationToken = default);
    Task<CurrentDraftState> UpdateAsync(int draftId, UpdateDraftCommand command, CancellationToken cancellationToken = default);
    Task<CurrentDraftState> ApplyDefaultRankingAsync(int draftId, int? rankingId, int callerAccountId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<CurrentDraftState> SetPlayerPoolAsync(int draftId, SetPlayerPoolCommand command, CancellationToken cancellationToken = default);
    Task StartAsync(int draftId, CancellationToken cancellationToken = default);
    Task PauseAsync(int draftId, CancellationToken cancellationToken = default);
    Task EndAsync(int draftId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    Task<DraftPick?> GetCurrentPickAsync(int draftId, CancellationToken cancellationToken = default);
    Task<IPagedList<PlayerDraftRanking>> GetDraftRankingsAsync(int draftId, GetDraftRankingsQuery query, CancellationToken cancellationToken = default);
    Task<CurrentDraftStateWithPick> DraftPlayerAsync(DraftPlayerCommand command, CancellationToken cancellationToken = default);
    IAsyncEnumerable<CurrentDraftStateWithPick> ResolveAutoPicksAsync(int draftId, CancellationToken cancellationToken = default);
    Task<CurrentDraftState> ReplacePickAsync(ReplaceDraftPickCommand command, CancellationToken cancellationToken = default);
    Task<CurrentDraftState> UndoLastPickAsync(int draftId, CancellationToken cancellationToken = default);
    Task<CurrentDraftState> ResetDraftAsync(int draftId, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<int>> GetFavouritePlayerIdsAsync(int draftId, int accountId, CancellationToken cancellationToken = default);
    Task<bool> ToggleFavouriteAsync(int draftId, int playerId, int accountId, CancellationToken cancellationToken = default);
}


public record CurrentDraftState(
    Draft Draft,
    DraftPick? NextPick
);

public sealed record CurrentDraftStateWithPick(
    Draft Draft,
    DraftPick? NextPick,
    DraftPick CompletedPick
) : CurrentDraftState(Draft, NextPick);