using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetHighlightsQuery : QueryBase
{
    /// <summary>When set, only highlights tagged with this account.</summary>
    public int? AccountId { get; set; }
}
