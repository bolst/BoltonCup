namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to set a team's goal and win songs. A null song clears that selection.</summary>
public record UpdateTeamSongsRequest
{
    /// <summary>Gets the picked goal song, or null to clear it.</summary>
    public MusicTrackDto? GoalSong { get; init; }

    /// <summary>Gets the picked win song, or null to clear it.</summary>
    public MusicTrackDto? WinSong { get; init; }
}
