using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a player's availability for a single game.</summary>
public record GameAvailabilityDto
{
    /// <summary>Gets the game the availability applies to.</summary>
    public required int GameId { get; init; }

    /// <summary>Gets the player's availability for the game.</summary>
    public required GameAvailability Availability { get; init; }
}
