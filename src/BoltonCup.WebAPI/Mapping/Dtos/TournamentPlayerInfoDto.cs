namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player's pre-tournament info and the games of their assigned team.</summary>
public record TournamentPlayerInfoDto
{
    /// <summary>Gets the player's availability for each of their team's games.</summary>
    public IReadOnlyList<GameAvailabilityDto> GameAvailability { get; init; } = [];

    /// <summary>Gets the player's song request, if one has been selected.</summary>
    public MusicTrackDto? Song { get; init; }

    /// <summary>Gets the games of the player's assigned team (empty if the player has no team yet).</summary>
    public IReadOnlyList<GameDto> Games { get; init; } = [];

    /// <summary>Gets the player's assigned team, if exists.</summary>
    public TeamBriefDto? CurrentTeam { get; init; }

    /// <summary>Gets the team the requester GMs in this tournament, if any (enables GM-only song controls).</summary>
    public ManagedTeamDto? ManagedTeam { get; init; }
}

/// <summary>The team a GM manages in a tournament, with its current goal/win song selections.</summary>
public record ManagedTeamDto
{
    /// <summary>Gets the managed team's id.</summary>
    public int TeamId { get; init; }

    /// <summary>Gets the managed team's name.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Gets the team's currently selected goal song, if any.</summary>
    public MusicTrackDto? GoalSong { get; init; }

    /// <summary>Gets the team's currently selected win song, if any.</summary>
    public MusicTrackDto? WinSong { get; init; }
}
