using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update a game's state.</summary>
public record UpdateGameStateRequest
{
    /// <summary>Gets or sets the new game state.</summary>
    public GameState State { get; set; }
}
