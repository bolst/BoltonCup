namespace BoltonCup.Core;

/// <summary>
/// Refreshes the materialized views backing skater and goalie statistics. These views are not updated
/// automatically when games/goals/penalties change — they must be refreshed explicitly (e.g. when a
/// game ends, or via the manual trigger in the Admin app).
/// </summary>
public interface IStatisticsRefreshService
{
    /// <summary>Refreshes the skater and goalie stat materialized views.</summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
