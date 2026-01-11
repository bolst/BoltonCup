using BoltonCup.Core.Queries;

namespace BoltonCup.Core;

public sealed record GetSkaterStatsQuery : DefaultPaginationQuery
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
    public string? Position { get; set; }
}