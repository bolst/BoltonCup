using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IDraftService
{
    Task<IPagedList<Draft>> GetAsync(GetDraftsQuery query, CancellationToken cancellationToken = default);
    Task<Draft?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateDraftCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateDraftCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    Task<DraftPick?> GetCurrentPickAsync(int draftId, CancellationToken cancellationToken = default);
    Task DraftPlayerAsync(DraftPlayerCommand command, CancellationToken cancellationToken = default);
}