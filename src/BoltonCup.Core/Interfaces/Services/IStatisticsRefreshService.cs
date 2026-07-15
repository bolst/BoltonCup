namespace BoltonCup.Core;

/// <summary>
/// Recomputes the skater and goalie game-log tables from the underlying game data. The tables are not
/// updated automatically when games/goals/penalties change — a refresh must be triggered explicitly
/// (e.g. when a goal/penalty is recorded, when a game ends, or via the manual trigger in the Admin app).
/// </summary>
public interface IStatisticsRefreshService
{
    /// <summary>Recomputes and upserts the skater and goalie game-log tables.</summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
