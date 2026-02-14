using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetTeamsQuery : QueryBase
{
    public int? TournamentId { get; init; }
}