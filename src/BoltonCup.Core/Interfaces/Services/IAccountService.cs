using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IAccountService
{
    Task UpdateAsync(UpdateAccountCommand command, CancellationToken cancellationToken = default);
    Task UpdateAvatarAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
}