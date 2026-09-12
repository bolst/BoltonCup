using BoltonCup.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Application.Services;

/// <summary>
/// Runs an immediate refresh on startup (so stats aren't empty before the first game event triggers
/// one) and then once every 24 hours as a safety net, since <see cref="IStatisticsRefreshService"/> is
/// otherwise only refreshed on demand (game writes, the manual Admin trigger).
/// </summary>
public class StatisticsRefreshBackgroundService(IServiceProvider _serviceProvider, ILogger<StatisticsRefreshBackgroundService> _logger)
    : BackgroundService
{
    static readonly TimeSpan RefreshInterval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(RefreshInterval);

        do
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stats = scope.ServiceProvider.GetRequiredService<IStatisticsRefreshService>();
                await stats.RefreshAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh statistics.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
