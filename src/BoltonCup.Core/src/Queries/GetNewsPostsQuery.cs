using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetNewsPostsQuery : QueryBase
{
    /// <summary>When set, only posts carrying a Label tag with this name (case-insensitive).</summary>
    public string? Label { get; set; }
}
