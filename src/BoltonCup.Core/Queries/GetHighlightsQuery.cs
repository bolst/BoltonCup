using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetHighlightsQuery : QueryBase
{
    /// <summary>When set, only highlights tagged with this player.</summary>
    public int? PlayerId { get; set; }
}
