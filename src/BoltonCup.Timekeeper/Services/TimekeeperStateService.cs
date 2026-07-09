using System.Text.Json;
using BoltonCup.Sdk;
using Microsoft.JSInterop;
using MudBlazor;

namespace BoltonCup.Timekeeper.Services;

public class TimekeeperStateService : IDisposable
{
    private const string AutoplayGoalSongKey = "bc:autoplayGoalSong";
    private const string AutoplayPenaltySongKey = "bc:autoplayPenaltySong";
    private const string AutoplayWinSongKey = "bc:autoplayWinSong";
    private const string NormalizeAudioKey = "bc:normalizeAudio";

    private readonly IBoltonCupApi _api;
    private readonly ISnackbar _snackbar;
    private readonly ILogger<TimekeeperStateService> _logger;
    private readonly IOfflineStore _offlineStore;
    private readonly SyncService _syncService;
    private readonly IJSRuntime _js;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private int _pendingHomeGoals;
    private int _pendingAwayGoals;

    public GameSingleDto? Game { get; private set; }
    public List<PlayerDto> HomePlayers { get; private set; } = [];
    public List<PlayerDto> AwayPlayers { get; private set; } = [];
    public int CurrentPeriod { get; private set; } = 1;
    public bool IsLoading { get; private set; }
    public bool IsOnline => _syncService.IsOnline;

    /// <summary>When true, clicking a team's Goal button plays that team's goal song. Persisted to localStorage.</summary>
    public bool AutoplayGoalSong { get; private set; } = true;

    /// <summary>When true, clicking a team's Penalty button plays that team's penalty song. Persisted to localStorage.</summary>
    public bool AutoplayPenaltySong { get; private set; } = true;

    /// <summary>When true, ending a game plays the winning team's win song. Off by default. Persisted to localStorage.</summary>
    public bool AutoplayWinSong { get; private set; }

    /// <summary>When true, music downloads measure loudness so playback can be level-normalized. Persisted to localStorage.</summary>
    public bool NormalizeAudio { get; private set; } = true;

    private bool _settingsLoaded;

    public event Action? OnStateChanged;

    public int HomeScore => (Game?.HomeTeam?.Goals ?? 0) + _pendingHomeGoals;
    public int AwayScore => (Game?.AwayTeam?.Goals ?? 0) + _pendingAwayGoals;

    public string CurrentPeriodLabel => PeriodLabel(CurrentPeriod);

    public TimekeeperStateService(
        IBoltonCupApi api,
        ISnackbar snackbar,
        ILogger<TimekeeperStateService> logger,
        IOfflineStore offlineStore,
        SyncService syncService,
        IJSRuntime js)
    {
        _api = api;
        _snackbar = snackbar;
        _logger = logger;
        _offlineStore = offlineStore;
        _syncService = syncService;
        _js = js;
        _syncService.OnSyncCompleted += HandleSyncCompleted;
    }

    /// <summary>Reads the persisted music preferences once (missing → default on).</summary>
    private async Task EnsureSettingsLoadedAsync()
    {
        if (_settingsLoaded) return;
        _settingsLoaded = true;
        try
        {
            var autoplay = await _js.InvokeAsync<string?>("localStorage.getItem", AutoplayGoalSongKey);
            AutoplayGoalSong = autoplay != "0";
            var autoplayPenalty = await _js.InvokeAsync<string?>("localStorage.getItem", AutoplayPenaltySongKey);
            AutoplayPenaltySong = autoplayPenalty != "0";
            var autoplayWin = await _js.InvokeAsync<string?>("localStorage.getItem", AutoplayWinSongKey);
            AutoplayWinSong = autoplayWin == "1";
            var normalize = await _js.InvokeAsync<string?>("localStorage.getItem", NormalizeAudioKey);
            NormalizeAudio = normalize != "0";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read music preferences");
        }
    }

    /// <summary>Ensures the persisted music preferences have been read (safe to call before reading them).</summary>
    public Task EnsureSettingsLoadedPublicAsync() => EnsureSettingsLoadedAsync();

    public async Task SetAutoplayGoalSongAsync(bool value)
    {
        AutoplayGoalSong = value;
        NotifyStateChanged();
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AutoplayGoalSongKey, value ? "1" : "0");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist autoplay goal song preference");
        }
    }

    public async Task SetAutoplayPenaltySongAsync(bool value)
    {
        AutoplayPenaltySong = value;
        NotifyStateChanged();
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AutoplayPenaltySongKey, value ? "1" : "0");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist autoplay penalty song preference");
        }
    }

    public async Task SetAutoplayWinSongAsync(bool value)
    {
        AutoplayWinSong = value;
        NotifyStateChanged();
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AutoplayWinSongKey, value ? "1" : "0");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist autoplay win song preference");
        }
    }

    public async Task SetNormalizeAudioAsync(bool value)
    {
        NormalizeAudio = value;
        NotifyStateChanged();
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", NormalizeAudioKey, value ? "1" : "0");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist normalize audio preference");
        }
    }

    public static string PeriodLabel(int period) => period switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        4 => "OT",
        _ => $"OT{period - 3}",
    };

    public IEnumerable<PlayerDto> AllPlayers => HomePlayers.Concat(AwayPlayers);

    public async Task LoadGameAsync(int gameId)
    {
        IsLoading = true;
        _pendingHomeGoals = 0;
        _pendingAwayGoals = 0;
        await EnsureSettingsLoadedAsync();
        NotifyStateChanged();

        try
        {
            Game = await _api.GetGameByIdAsync(gameId);

            var loadHome = Game.HomeTeam is null
                ? Task.FromResult<List<PlayerDto>>([])
                : FetchRosterAsync(Game.HomeTeam.Id);
            var loadAway = Game.AwayTeam is null
                ? Task.FromResult<List<PlayerDto>>([])
                : FetchRosterAsync(Game.AwayTeam.Id);

            await Task.WhenAll(loadHome, loadAway);
            HomePlayers = await loadHome;
            AwayPlayers = await loadAway;

            await _offlineStore.CacheGameAsync(Game);
            await _offlineStore.CacheRostersAsync(gameId, HomePlayers, AwayPlayers);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API unavailable loading game {GameId}, trying offline cache", gameId);

            var cached = await _offlineStore.TryLoadGameAsync(gameId);
            if (cached is not null)
            {
                Game = cached;
                var (home, away) = await _offlineStore.TryLoadRostersAsync(gameId);
                HomePlayers = home;
                AwayPlayers = away;
                _snackbar.Add("Loaded from offline cache", Severity.Warning);
            }
            else
            {
                _logger.LogError(ex, "Failed to load game {GameId} and no offline cache available", gameId);
                _snackbar.Add($"Failed to load game: {ex.Message}", Severity.Error);
                Game = null;
            }
        }

        if (Game is not null)
        {
            var maxEventPeriod = 0;
            if (Game.Goals.Count > 0)
                maxEventPeriod = Math.Max(maxEventPeriod, Game.Goals.Max(g => g.Period));
            if (Game.Penalties.Count > 0)
                maxEventPeriod = Math.Max(maxEventPeriod, Game.Penalties.Max(p => p.Period));
            CurrentPeriod = Math.Max(1, maxEventPeriod);
        }

        _syncService.SetActiveGame(Game?.Id);
        IsLoading = false;
        NotifyStateChanged();
    }

    private async Task<List<PlayerDto>> FetchRosterAsync(int teamId)
    {
        var result = await _api.GetPlayersAsync(teamId: teamId, size: 100);
        return [..result.Items.OrderBy(p => p.JerseyNumber ?? int.MaxValue)];
    }

    public async Task RefreshGameAsync()
    {
        if (Game is null) return;
        try
        {
            Game = await _api.GetGameByIdAsync(Game.Id);
            _pendingHomeGoals = 0;
            _pendingAwayGoals = 0;
            await _offlineStore.CacheGameAsync(Game);
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh game {GameId}", Game.Id);
        }
    }

    public void SetCurrentPeriod(int period)
    {
        if (period < 1) period = 1;
        if (period == CurrentPeriod) return;
        CurrentPeriod = period;
        NotifyStateChanged();
    }

    public async Task UpdateGameStateAsync(GameState state, bool includePlayerSongs = true)
    {
        if (Game is null) return;
        var request = new UpdateGameStateRequest { State = state, IncludePlayerSongs = includePlayerSongs };
        if (_syncService.IsOnline)
        {
            try
            {
                await _api.UpdateGameStateAsync(Game.Id, request);
                await RefreshGameAsync();
                _snackbar.Add($"Game state set to {state}.", Severity.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update game state");
                _snackbar.Add($"Failed to update game state: {ex.Message}", Severity.Error);
            }
        }
        else
        {
            await EnqueueAsync("GameState", request);
            _snackbar.Add("Game state queued — will sync when online", Severity.Warning);
        }
    }

    public async Task<bool> AddGoalAsync(CreateGoalRequest request)
    {
        if (Game is null) return false;
        if (_syncService.IsOnline)
        {
            try
            {
                await _api.AddGoalAsync(Game.Id, request);
                await RefreshGameAsync();
                _snackbar.Add("Goal recorded.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add goal");
                _snackbar.Add($"Failed to record goal: {ex.Message}", Severity.Error);
                return false;
            }
        }
        else
        {
            if (request.TeamId == Game.HomeTeam?.Id) _pendingHomeGoals++;
            else if (request.TeamId == Game.AwayTeam?.Id) _pendingAwayGoals++;
            await EnqueueAsync("AddGoal", request);
            _snackbar.Add("Goal queued — will sync when online", Severity.Warning);
            NotifyStateChanged();
            return true;
        }
    }

    public async Task<bool> DeleteGoalAsync(int goalId)
    {
        if (Game is null) return false;
        if (_syncService.IsOnline)
        {
            try
            {
                await _api.DeleteGoalAsync(Game.Id, goalId);
                await RefreshGameAsync();
                _snackbar.Add("Goal deleted.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete goal {GoalId}", goalId);
                _snackbar.Add($"Failed to delete goal: {ex.Message}", Severity.Error);
                return false;
            }
        }
        else
        {
            await EnqueueAsync("DeleteGoal", new { id = goalId });
            _snackbar.Add("Delete queued — will sync when online", Severity.Warning);
            return true;
        }
    }

    public async Task<bool> AddPenaltyAsync(CreatePenaltyRequest request)
    {
        if (Game is null) return false;
        if (_syncService.IsOnline)
        {
            try
            {
                await _api.AddPenaltyAsync(Game.Id, request);
                await RefreshGameAsync();
                _snackbar.Add("Penalty recorded.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add penalty");
                _snackbar.Add($"Failed to record penalty: {ex.Message}", Severity.Error);
                return false;
            }
        }
        else
        {
            await EnqueueAsync("AddPenalty", request);
            _snackbar.Add("Penalty queued — will sync when online", Severity.Warning);
            return true;
        }
    }

    public async Task<bool> DeletePenaltyAsync(int penaltyId)
    {
        if (Game is null) return false;
        if (_syncService.IsOnline)
        {
            try
            {
                await _api.DeletePenaltyAsync(Game.Id, penaltyId);
                await RefreshGameAsync();
                _snackbar.Add("Penalty deleted.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete penalty {PenaltyId}", penaltyId);
                _snackbar.Add($"Failed to delete penalty: {ex.Message}", Severity.Error);
                return false;
            }
        }
        else
        {
            await EnqueueAsync("DeletePenalty", new { id = penaltyId });
            _snackbar.Add("Delete queued — will sync when online", Severity.Warning);
            return true;
        }
    }

    public async Task<bool> SetStarsAsync(SetGameStarsRequest request)
    {
        if (Game is null) return false;
        if (_syncService.IsOnline)
        {
            try
            {
                await _api.SetGameStarsAsync(Game.Id, request);
                await RefreshGameAsync();
                _snackbar.Add("Game stars saved.", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set game stars");
                _snackbar.Add($"Failed to set stars: {ex.Message}", Severity.Error);
                return false;
            }
        }
        else
        {
            await EnqueueAsync("Stars", request);
            _snackbar.Add("Stars queued — will sync when online", Severity.Warning);
            return true;
        }
    }

    private async Task EnqueueAsync<T>(string eventType, T payload)
    {
        if (Game is null) return;
        var record = new OfflineEventRecord
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            CreatedAt = DateTime.UtcNow,
        };
        await _offlineStore.EnqueueAsync(Game.Id, record);
    }

    private void HandleSyncCompleted()
    {
        _ = RefreshGameAsync();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void Dispose()
    {
        _syncService.OnSyncCompleted -= HandleSyncCompleted;
    }
}
