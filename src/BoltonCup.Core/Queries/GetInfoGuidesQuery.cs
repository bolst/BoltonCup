using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetInfoGuidesQuery : DefaultPaginationQuery
{
    public int? TournamentId { get; init; }
}