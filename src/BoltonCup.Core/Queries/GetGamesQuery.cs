using BoltonCup.Core.Queries;
using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetGamesQuery : DefaultPaginationQuery
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
}