namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to create or update the authenticated user's pre-tournament player info.</summary>
public record UpdateTournamentPlayerInfoRequest
{
    /// <summary>Gets or sets the player's availability for each of their team's games.</summary>
    public List<GameAvailabilityDto> GameAvailability { get; set; } = [];

    /// <summary>Gets or sets the player's song request, if one has been selected.</summary>
    public MusicTrackDto? Song { get; set; }
}
