using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface ICustomRankingService
{
    Task<IReadOnlyList<CustomRanking>> GetForAccountAsync(int accountId, int? tournamentId = null, CancellationToken cancellationToken = default);
    Task<CustomRanking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateCustomRankingCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateCustomRankingCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
