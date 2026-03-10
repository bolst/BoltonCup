namespace BoltonCup.Core;

public interface IAccountService
{
    Task UpdateProfilePictureAsync(int accountId, string tempKey, CancellationToken cancellationToken = default);
}