using BoltonCup.Sdk;

namespace BoltonCup.WebClient.Services;

public class BcStateService(IBoltonCupApi _api)
{
    static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    SystemContextDto? _context;
    DateTime _fetchedAtUtc;

    List<GameDto>? _completedGames;
    DateTime _completedGamesFetchedAtUtc;

    public Task<SystemContextDto> Context => GetContextAsync();

    /// <summary>
    /// Whether the active tournament has started (i.e. its start date is in the past),
    /// which drives the home page into "started" mode. Evaluated against the current
    /// time from the cached context, so the transition surfaces on the next navigation
    /// after the start date passes.
    /// </summary>
    public async Task<bool> IsActiveTournamentStartedAsync()
    {
        var context = await GetContextAsync();
        return context.ActiveTournament?.StartDate is { } startDate
            && DateTime.SpecifyKind(startDate, DateTimeKind.Utc) <= DateTime.UtcNow;
    }

    /// <summary>
    /// Completed games for the active tournament, ordered most-recent first.
    /// Cached with the same short TTL as the context so navigating back into
    /// "started" mode reuses the last fetch instead of re-querying every time.
    /// </summary>
    public async Task<IReadOnlyList<GameDto>> GetCompletedGamesAsync()
    {
        if (_completedGames is not null && DateTime.UtcNow - _completedGamesFetchedAtUtc < CacheDuration)
        {
            return _completedGames;
        }

        var context = await GetContextAsync();
        if (context.ActiveTournament is not { } tournament)
        {
            return _completedGames = [];
        }

        var games = await _api.GetGamesAsync(tournamentId: tournament.Id, size: 50);
        _completedGames = games.Items
            .Where(g => g.GameState == GameState.Completed)
            .OrderByDescending(g => g.GameTime)
            .ToList();
        _completedGamesFetchedAtUtc = DateTime.UtcNow;
        return _completedGames;
    }

    async Task<SystemContextDto> GetContextAsync()
    {
        if (_context is not null && DateTime.UtcNow - _fetchedAtUtc < CacheDuration)
        {
            return _context;
        }
        var context = await _api.GetSystemContextAsync();
        _context = context;
        _fetchedAtUtc = DateTime.UtcNow;
        return context;
    }
}