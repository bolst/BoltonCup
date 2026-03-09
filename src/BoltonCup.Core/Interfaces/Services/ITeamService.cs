namespace BoltonCup.Core;

public interface ITeamService
{
    Task UpdateTeamLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateTeamBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
}