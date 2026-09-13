namespace BoltonCup.Core;

public interface ITeamService
{
    Task<IPagedList<Team>> GetAllAsync(GetTeamsQuery query, CancellationToken cancellationToken = default);
    Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateLogoAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int teamId, string tempKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the team's goal, win and penalty songs from picked tracks. Each track is registered in the
    /// tournament's music library (the fetcher downloads it); a null clears that song without touching any track row.
    /// </summary>
    Task UpdateSongsAsync(int teamId, MusicTrack? goalSong, MusicTrack? winSong, MusicTrack? penaltySong, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the team's goal, win and penalty songs to existing tournament music-pool tracks by id (or null to
    /// clear). Each non-null id must belong to the team's tournament. Used by the admin pool-picker, which selects
    /// already-present tracks rather than queuing a Spotify download.
    /// </summary>
    Task SetSongTracksAsync(int teamId, int? goalTrackId, int? winTrackId, int? penaltyTrackId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the team's set of general managers with the given accounts, adding/removing memberships as needed.</summary>
    Task SetGeneralManagersAsync(int teamId, IReadOnlyCollection<int> accountIds, CancellationToken cancellationToken = default);

    /// <summary>Whether the account is a GM of the team.</summary>
    Task<bool> CanManageAsync(int teamId, int accountId, CancellationToken cancellationToken = default);
}