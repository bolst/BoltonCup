using BoltonCup.Core.Queries;

namespace BoltonCup.Core;

public sealed record GetGamesQuery : DefaultPaginationQuery
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
}