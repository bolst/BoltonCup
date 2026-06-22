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
}
