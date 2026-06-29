using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player's availability for a single game, including the game time. A null availability means the player has not responded.</summary>
public sealed record PlayerGameAvailabilityDto
{
    /// <summary>Gets the game the availability applies to.</summary>
    public required int GameId { get; init; }

    /// <summary>Gets the scheduled time of the game.</summary>
    public required DateTime GameTime { get; init; }

    /// <summary>Gets the player's availability for the game, or null if they have not responded.</summary>
    public GameAvailability? Availability { get; init; }
}