using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetPlayersQuery : DefaultPaginationQuery
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
}