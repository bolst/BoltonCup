namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to set a team's goal, win and penalty songs. A null song clears that selection.</summary>
public record UpdateTeamSongsRequest
{
    /// <summary>Gets the picked goal song, or null to clear it.</summary>
    public MusicTrackDto? GoalSong { get; init; }

    /// <summary>Gets the picked win song, or null to clear it.</summary>
    public MusicTrackDto? WinSong { get; init; }

    /// <summary>Gets the picked penalty song, or null to clear it.</summary>
    public MusicTrackDto? PenaltySong { get; init; }
}