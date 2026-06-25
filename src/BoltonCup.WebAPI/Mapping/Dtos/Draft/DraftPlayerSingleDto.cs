namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed player DTO for the draft, extending the single-player view with per-game availability.</summary>
public sealed record DraftPlayerSingleDto : PlayerSingleDto
{
    /// <summary>Gets the player's availability for each game in their tournament, ordered by game time.</summary>
    public IReadOnlyList<PlayerGameAvailabilityDto> GameAvailabilities { get; init; } = [];
}
