using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetGoalieStatsQuery : QueryBase
{
    public int? TournamentId { get; set; }
    public List<int>? TeamIds { get; set; }
    public int? GameId { get; set; }
}