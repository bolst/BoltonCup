using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// Refreshes the skater/goalie stat materialized views. Uses a short-lived context from the factory so it
/// is safe to call from the Blazor Server Admin app (no shared circuit-wide context) as well as the API.
/// </summary>
public sealed class StatisticsRefreshService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory)
    : IStatisticsRefreshService
{
    private static readonly string[] Views =
    [
        "core.mv_skater_game_logs",
        "core.mv_goalie_game_logs",
    ];

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // A plain (non-concurrent) refresh: the stat views hold a single tournament's data, so this runs in
        // well under a second and the brief lock is a non-issue. Non-concurrent also avoids the unique-index
        // and no-transaction-block requirements that REFRESH ... CONCURRENTLY imposes.
        foreach (var view in Views)
        {
            await db.Database.ExecuteSqlRawAsync("REFRESH MATERIALIZED VIEW " + view + ";", cancellationToken);
        }
    }
}
