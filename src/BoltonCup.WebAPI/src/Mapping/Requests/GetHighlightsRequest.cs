namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of recent video highlights across all tournaments.</summary>
public record GetHighlightsRequest : RequestBase
{
    /// <summary>When set, only highlights tagged with this account.</summary>
    public int? AccountId { get; set; }
}
