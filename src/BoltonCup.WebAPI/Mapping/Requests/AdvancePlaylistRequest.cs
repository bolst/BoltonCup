namespace BoltonCup.WebAPI.Mapping;

/// <summary>Reports that a playlist track has started playing, so the shared tournament queue advances.</summary>
public record AdvancePlaylistRequest
{
    /// <summary>Gets the library track id that just started playing.</summary>
    public int MusicTrackId { get; init; }
}