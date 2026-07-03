using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface ITradeService
{
    Task<IReadOnlyList<Trade>> GetByTournamentAsync(int tournamentId, CancellationToken cancellationToken = default);
    Task<Trade?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateTradeCommand command, CancellationToken cancellationToken = default);
    Task AcceptAsync(int tradeId, int accountId, CancellationToken cancellationToken = default);
    Task DeclineAsync(int tradeId, int accountId, CancellationToken cancellationToken = default);
    Task CancelAsync(int tradeId, int accountId, bool isAdmin, CancellationToken cancellationToken = default);
    Task ApproveAsync(int tradeId, int accountId, CancellationToken cancellationToken = default);
}
