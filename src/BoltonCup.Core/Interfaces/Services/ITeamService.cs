namespace BoltonCup.Core;

public interface ITeamService
{
    Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the team's goal and win songs from picked tracks. Each track is registered in the tournament's
    /// music library (the fetcher downloads it); a null clears that song without touching any track row.
    /// </summary>
    Task UpdateSongsAsync(int teamId, MusicTrack? goalSong, MusicTrack? winSong, CancellationToken cancellationToken = default);

    /// <summary>Replaces the team's set of general managers with the given accounts, adding/removing memberships as needed.</summary>
    Task SetGeneralManagersAsync(int teamId, IReadOnlyCollection<int> accountIds, CancellationToken cancellationToken = default);
}