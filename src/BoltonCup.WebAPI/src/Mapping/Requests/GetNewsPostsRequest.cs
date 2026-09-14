namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of published news posts.</summary>
public record GetNewsPostsRequest : RequestBase
{
    /// <summary>When set, only posts tagged with a label of this name (case-insensitive).</summary>
    public string? Label { get; set; }
}
