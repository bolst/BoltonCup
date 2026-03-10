namespace BoltonCup.Core;

public interface ITeamService
{
    Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
}