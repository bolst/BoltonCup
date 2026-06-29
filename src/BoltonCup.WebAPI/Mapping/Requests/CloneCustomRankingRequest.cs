namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to clone an accessible ranking into a new one owned by the caller.</summary>
public record CloneCustomRankingRequest
{
    /// <summary>Gets or sets the title for the clone. Falls back to "Copy of {source}" when empty.</summary>
    public string? Title { get; set; }
}
