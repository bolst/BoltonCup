using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update a game's state.</summary>
public record UpdateGameStateRequest
{
    /// <summary>Gets or sets the new game state.</summary>
    public GameState State { get; set; }

    /// <summary>
    /// When transitioning into <see cref="GameState.InProgress"/>, whether to inject the participating
    /// players' requested songs at the front of the shared queue. Ignored for other transitions.
    /// </summary>
    public bool IncludePlayerSongs { get; set; } = true;
}