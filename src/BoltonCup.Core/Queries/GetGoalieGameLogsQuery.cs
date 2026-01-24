using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core;

public sealed record GetGoalieGameLogsQuery : DefaultSortablePaginationQuery
{
    public required int GameId { get; set; }
    public string? HomeOrAway { get; set; }
    public int? TeamId { get; set; }
}