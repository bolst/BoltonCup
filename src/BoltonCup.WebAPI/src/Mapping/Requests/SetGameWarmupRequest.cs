namespace BoltonCup.WebAPI.Mapping;

/// <summary>Sets a game's warmup playlist to the given tournament pool track ids, in playback order.</summary>
public record SetGameWarmupRequest
{
    /// <summary>Gets the ordered pool track ids that make up the warmup playlist. Empty clears it.</summary>
    public IReadOnlyList<int> TrackIds { get; init; } = [];
}