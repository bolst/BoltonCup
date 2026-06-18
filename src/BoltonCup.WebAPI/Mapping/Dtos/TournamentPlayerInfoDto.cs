namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player's pre-tournament info and the games of their assigned team.</summary>
public record TournamentPlayerInfoDto
{
    /// <summary>Gets or sets the serialized player-info payload (game availability and song request).</summary>
    public string? Payload { get; set; }

    /// <summary>Gets the games of the player's assigned team (empty if the player has no team yet).</summary>
    public IReadOnlyList<GameDto> Games { get; init; } = [];
}
