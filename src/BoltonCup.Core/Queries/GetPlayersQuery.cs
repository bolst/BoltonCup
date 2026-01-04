using BoltonCup.Core.Queries;

namespace BoltonCup.Core;

public sealed record GetPlayersQuery : PaginationQueryBase
{
    public int? TournamentId { get; set; }
}