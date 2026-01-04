using BoltonCup.Core.Queries;

namespace BoltonCup.Core;

public sealed record GetTeamsQuery : PaginationQueryBase
{
    public int? TournamentId { get; init; }
}