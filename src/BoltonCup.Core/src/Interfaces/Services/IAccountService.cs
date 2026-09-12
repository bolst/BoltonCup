using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IAccountService
{
    Task<IPagedList<Account>> GetAllAsync(GetAccountsQuery query, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateAccountCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateAccountCommand command, CancellationToken cancellationToken = default);
    Task UpdateAvatarAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdatePreviousTeamLogoAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
}