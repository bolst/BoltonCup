using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetSkaterStatsQuery : DefaultSortablePaginationQuery
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
    public string? Position { get; set; }
}