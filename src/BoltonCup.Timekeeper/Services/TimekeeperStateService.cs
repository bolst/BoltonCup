using BoltonCup.Sdk;
using MudBlazor;

namespace BoltonCup.Timekeeper.Services;

public class TimekeeperStateService
{
    private readonly IBoltonCupApi _api;
    private readonly ISnackbar _snackbar;
    private readonly ILogger<TimekeeperStateService> _logger;

    public GameSingleDto? Game { get; private set; }
    public List<PlayerDto> HomePlayers { get; private set; } = [];
    public List<PlayerDto> AwayPlayers { get; private set; } = [];
    public int CurrentPeriod { get; private set; } = 1;
    public bool IsLoading { get; private set; }

    public event Action? OnStateChanged;

    public int HomeScore => Game?.HomeTeam?.Goals ?? 0;
    public int AwayScore => Game?.AwayTeam?.Goals ?? 0;

    public string CurrentPeriodLabel => PeriodLabel(CurrentPeriod);

    public TimekeeperStateService(
        IBoltonCupApi api,
        ISnackbar snackbar,
        ILogger<TimekeeperStateService> logger)
    {
        _api = api;
        _snackbar = snackbar;
        _logger = logger;
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

            var maxEventPeriod = 0;
            if (Game.Goals.Count > 0)
                maxEventPeriod = Math.Max(maxEventPeriod, Game.Goals.Max(g => g.Period));
            if (Game.Penalties.Count > 0)
                maxEventPeriod = Math.Max(maxEventPeriod, Game.Penalties.Max(p => p.Period));
            CurrentPeriod = Math.Max(1, maxEventPeriod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load game {GameId}", gameId);
            _snackbar.Add($"Failed to load game: {ex.Message}", Severity.Error);
            Game = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
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
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh game {GameId}", Game.Id);
            _snackbar.Add($"Failed to refresh: {ex.Message}", Severity.Error);
        }
    }

    public void SetCurrentPeriod(int period)
    {
        if (period < 1) period = 1;
        if (period == CurrentPeriod) return;
        CurrentPeriod = period;
        NotifyStateChanged();
    }

    public async Task UpdateGameStateAsync(GameState state)
    {
        if (Game is null) return;
        try
        {
            await _api.UpdateGameStateAsync(Game.Id, new UpdateGameStateRequest { State = state });
            await RefreshGameAsync();
            _snackbar.Add($"Game state set to {state}.", Severity.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game state");
            _snackbar.Add($"Failed to update game state: {ex.Message}", Severity.Error);
        }
    }

    public async Task<bool> AddGoalAsync(CreateGoalRequest request)
    {
        if (Game is null) return false;
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

    public async Task<bool> DeleteGoalAsync(int goalId)
    {
        if (Game is null) return false;
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

    public async Task<bool> AddPenaltyAsync(CreatePenaltyRequest request)
    {
        if (Game is null) return false;
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

    public async Task<bool> DeletePenaltyAsync(int penaltyId)
    {
        if (Game is null) return false;
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

    public async Task<bool> SetStarsAsync(SetGameStarsRequest request)
    {
        if (Game is null) return false;
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

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
