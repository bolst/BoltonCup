namespace BoltonCup.Core;

public interface IAccountService
{
    Task UpdateAvatarAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
}