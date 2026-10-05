using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetAlbumsQuery : QueryBase
{
    /// <summary>When set, only albums carrying a Label tag with this name (case-insensitive).</summary>
    public string? Label { get; set; }
}