using BoltonCup.Core.Queries;

namespace BoltonCup.Core;

public sealed record GetTeamsQuery : DefaultPaginationQuery
{
    public int? TournamentId { get; init; }
}