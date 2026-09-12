namespace BoltonCup.WebAPI.Mapping;

/// <summary>A player's per-game availability for a tournament, keyed by account.</summary>
public sealed record PlayerAvailabilityDto
{
    /// <summary>Gets the account the availability belongs to.</summary>
    public required int AccountId { get; init; }

    /// <summary>Gets the player's availability for each game in the tournament, ordered by game time.</summary>
    public required IReadOnlyList<PlayerGameAvailabilityDto> GameAvailabilities { get; init; }
}