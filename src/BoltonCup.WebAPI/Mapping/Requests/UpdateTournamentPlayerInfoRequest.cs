namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to create or update the authenticated user's pre-tournament player info.</summary>
public record UpdateTournamentPlayerInfoRequest
{
    /// <summary>Gets or sets the serialized player-info payload (game availability and song request).</summary>
    public string? Payload { get; set; }
}
