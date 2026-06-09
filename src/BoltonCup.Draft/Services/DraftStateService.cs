using BoltonCup.Common;
using BoltonCup.Sdk;
using BoltonCup.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace BoltonCup.Draft.Services;


public class DraftStateService : IAsyncDisposable
{
    private readonly IBoltonCupApi _api;
    private readonly ILogger<DraftStateService> _logger;
    private readonly IJSRuntime _js;
    private readonly HubConnection _hubConnection;
    private readonly SemaphoreSlim _fetchLock;
    private readonly CancellationTokenSource _cts;

    private bool _disposed;
    private CancellationTokenSource? _pollCts;
    private CancellationTokenSource? _countdownCts;
    private int? _draftId;
    private Dictionary<int, int>? _customRankByPlayerId;

    public DraftSingleDto? Draft { get; private set; }
    public DraftPickSingleDto? CurrentPick { get; private set; }
    public List<DraftRankingDto> PlayerRankings { get; private set; } = [];

    public List<CustomRankingDto> AvailableCustomRankings { get; private set; } = [];
    public int? SelectedCustomRankingId { get; private set; }

    /// <summary>Seconds left on the current pick's clock, or null when no clock is running.</summary>
    public int? RemainingSeconds { get; private set; }

    public DraftConnectionState ConnectionState { get; private set; } = DraftConnectionState.Disconnected;

    public bool CanEditDraft => Draft?.CanEditDraft ?? false;
    public bool CanManageDraft => Draft?.CanManageDraft ?? false;

    public event Action? OnStateChanged;

    public DraftStateService(
        IBoltonCupApi api,
        ILogger<DraftStateService> logger,
        IJSRuntime js,
        IOptions<BoltonCupConfiguration> config
    )
    {
        _api = api;
        _logger = logger;
        _js = js;
        _fetchLock = new SemaphoreSlim(1, 1);
        _cts = new CancellationTokenSource();

        var hubUrl = $"{config.Value.ApiBaseUrl.TrimEnd('/')}{Hubs.Draft}";
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        ConnectionState = DraftConnectionState.Disconnected;

        _hubConnection.Reconnecting += OnReconnecting;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Closed += OnClosed;
        
        _hubConnection.On<DraftUpdateEventDto>(HubEvents.Draft.OnDraftUpdate, HandleDraftUpdate);
        _hubConnection.On<DraftStatus>(HubEvents.Draft.OnDraftStatusChange, HandleDraftStatusChange);
        _hubConnection.On<DraftPickMadeEventDto>(HubEvents.Draft.OnPickMade, HandlePickMade);
    }

    public async Task InitializeAsync(int draftId)
    {
        // switching drafts
        if (_draftId.HasValue && _draftId.Value != draftId && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveLiveDraft", _draftId.Value);
        }
        
        _draftId = draftId;

        await FetchStateAsync();

        await LoadCustomRankingsAsync(draftId);

        // a completed draft will not have any updates
        if (Draft is null or { Status: DraftStatus.Completed })
            return;

        await EnsureConnectedAndJoinedAsync(draftId);
    }


    private async Task LoadCustomRankingsAsync(int draftId)
    {
        if (Draft is null)
            return;

        try
        {
            var rankings = await _api.GetCustomRankingsAsync(Draft.Tournament.Id, _cts.Token);
            AvailableCustomRankings = rankings.ToList();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading custom rankings");
            AvailableCustomRankings = [];
        }

        var persisted = await GetPersistedRankingIdAsync(draftId);
        if (persisted is { } rankingId && AvailableCustomRankings.Any(r => r.Id == rankingId))
        {
            await SelectCustomRankingAsync(rankingId);
        }
        else
        {
            NotifyStateChanged();
        }
    }


    public async Task SelectCustomRankingAsync(int? rankingId)
    {
        SelectedCustomRankingId = rankingId;

        if (rankingId is null)
        {
            _customRankByPlayerId = null;
            await RemovePersistedRankingAsync();
            await FetchStateAsync();
            return;
        }

        try
        {
            var ranking = await _api.GetCustomRankingByIdAsync(rankingId.Value, _cts.Token);
            _customRankByPlayerId = ranking.Players.ToDictionary(p => p.Player.Id, p => p.Rank);
            await PersistRankingAsync(rankingId.Value);
            ApplyCustomRanking();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error applying custom ranking {RankingId}", rankingId);
        }

        NotifyStateChanged();
    }


    private void ApplyCustomRanking()
    {
        if (_customRankByPlayerId is not { } order)
            return;

        foreach (var item in PlayerRankings)
            item.DraftRanking = order.TryGetValue(item.Player.Id, out var rank) ? rank : int.MaxValue;

        PlayerRankings = PlayerRankings.OrderBy(p => p.DraftRanking).ToList();
    }


    private string RankingStorageKey(int draftId) => $"bc-draft-{draftId}-custom-ranking";

    private async Task<int?> GetPersistedRankingIdAsync(int draftId)
    {
        try
        {
            var value = await _js.InvokeAsync<string?>("localStorage.getItem", _cts.Token, RankingStorageKey(draftId));
            return int.TryParse(value, out var id) ? id : null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error reading persisted custom ranking");
            return null;
        }
    }

    private async Task PersistRankingAsync(int rankingId)
    {
        if (_draftId is not { } draftId)
            return;

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", _cts.Token, RankingStorageKey(draftId), rankingId.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error persisting custom ranking");
        }
    }

    private async Task RemovePersistedRankingAsync()
    {
        if (_draftId is not { } draftId)
            return;

        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", _cts.Token, RankingStorageKey(draftId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing persisted custom ranking");
        }
    }


    private Task OnReconnecting(Exception? error)
    {
        ConnectionState = DraftConnectionState.Reconnecting;
        NotifyStateChanged();
        StartFallbackPoll();
        return Task.CompletedTask;
    }


    private async Task OnReconnected(string? connectionId)
    {
        StopFallbackPoll();
        ConnectionState = DraftConnectionState.Connected;

        if (_draftId.HasValue)
        {
            await EnsureConnectedAndJoinedAsync(_draftId.Value);
        }

        await FetchStateAsync();
    }

    private Task OnClosed(Exception? error)
    {
        ConnectionState = DraftConnectionState.Disconnected;
        StopFallbackPoll();
        NotifyStateChanged();
        return Task.CompletedTask;
    }


    private async Task RunFallbackPollAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(8));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await FetchStateAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }


    private async Task HandleDraftUpdate(DraftUpdateEventDto eventDto)
    {
        Draft = eventDto.Draft;
        CurrentPick = eventDto.NextPick;
        SyncCountdown();
        NotifyStateChanged();

        // Settings changes can include a newly applied/cleared default ranking; refetch so the
        // player list order propagates to every connected GM (their own selection is reapplied on top).
        await FetchStateAsync();
    }


    private async Task HandleDraftStatusChange(DraftStatus newStatus)
    {
        if (Draft is null)
            return;
        Draft.Status = newStatus;

        if (newStatus == DraftStatus.Completed)
        {
            StopFallbackPoll();
            await StopHubAsync();
        }

        // On start/resume the server stamps a fresh ClockStartedAt; refetch so we pick it up.
        if (newStatus == DraftStatus.InProgress)
        {
            await FetchStateAsync();
        }

        SyncCountdown();
        NotifyStateChanged();
    }


    private void HandlePickMade(DraftPickMadeEventDto eventDto)
    {
        var existingPick = Draft?.DraftPicksByRound
            .Where(dpr => dpr.Round == eventDto.CompletedPick.Round)
            .SelectMany(pick => pick.Picks)
            .FirstOrDefault(p => p.OverallPick == eventDto.CompletedPick.OverallPick);
        if (existingPick is not null)
            existingPick.Player = eventDto.DraftedPlayer;

        var draftedPlayer = PlayerRankings
            .FirstOrDefault(p => p.Player.Id == eventDto.DraftedPlayer.Id);
        if (draftedPlayer is not null)
        {
            draftedPlayer.IsDrafted = true;
            draftedPlayer.DraftPick = eventDto.CompletedPick;
        }

        CurrentPick = eventDto.NextPick;

        SyncCountdown();
        NotifyStateChanged();
    }


    private async Task FetchStateAsync()
    {
        if (_draftId is not { } draftId || _disposed)
            return;

        if (!await _fetchLock.WaitAsync(TimeSpan.Zero))
            return;
        
        var token = _cts.Token;

        try
        {
            Draft = await _api.GetDraftByIdAsync(draftId, token);

            DraftPickSingleDto? currentPick = null;
            try
            {
                currentPick = await _api.GetCurrentDraftPickAsync(draftId, token);
            }
            catch (ApiException e) when (e.StatusCode == 204)
            {
                // no picks remaining — all picks are done
            }

            var playerRankings = await _api.GetDraftPlayerRankingsAsync(draftId, size: 200, cancellationToken: token);

            CurrentPick = currentPick;
            PlayerRankings = playerRankings.Items.ToList();
            ApplyCustomRanking();
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching draft state");
        }
        finally
        {
            _fetchLock.Release();
            if (!_disposed)
            {
                SyncCountdown();
                NotifyStateChanged();
            }
        }
    }


    private void SyncCountdown()
    {
        var shouldRun = Draft is { Status: DraftStatus.InProgress } && CurrentPick?.ClockStartedAt is not null;
        if (shouldRun)
        {
            RecomputeRemaining();
            StartCountdown();
        }
        else
        {
            StopCountdown();
            RemainingSeconds = null;
        }
    }

    private void RecomputeRemaining()
    {
        if (Draft?.SecondsPerPick is not { } perPick || CurrentPick?.ClockStartedAt is not { } startedAt)
        {
            RemainingSeconds = null;
            return;
        }

        var elapsed = (DateTime.UtcNow - startedAt.ToUniversalTime()).TotalSeconds;
        RemainingSeconds = Math.Max(0, perPick - (int)elapsed);
    }

    private void StartCountdown()
    {
        if (_countdownCts is not null)
            return;

        _countdownCts = new CancellationTokenSource();
        _ = RunCountdownAsync(_countdownCts.Token);
    }

    private async Task RunCountdownAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                RecomputeRemaining();
                NotifyStateChanged();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StopCountdown()
    {
        if (_countdownCts is null)
            return;

        _countdownCts.Cancel();
        _countdownCts.Dispose();
        _countdownCts = null;
    }

    
    private async Task EnsureConnectedAndJoinedAsync(int draftId)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }

        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinLiveDraft", draftId);
            ConnectionState = DraftConnectionState.Connected;   
        }
    }


    private void StartFallbackPoll()
    {
        StopFallbackPoll();
        _pollCts = new CancellationTokenSource();
        _ = RunFallbackPollAsync(_pollCts.Token);
    }

    private void StopFallbackPoll()
    {
        if (_pollCts is null) 
            return;
        
        _pollCts.Cancel();
        _pollCts.Dispose();
        _pollCts = null;
    }

    private async Task StopHubAsync()
    {
        if (_hubConnection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
        {
            if (_draftId.HasValue && _hubConnection.State == HubConnectionState.Connected)
                await _hubConnection.InvokeAsync("LeaveLiveDraft", _draftId.Value);

            await _hubConnection.StopAsync();
        }
    }

    
    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;

        await _cts.CancelAsync();
        _cts.Dispose();
        
        StopFallbackPoll();
        StopCountdown();
        _fetchLock.Dispose();

        await StopHubAsync();
        await _hubConnection.DisposeAsync();
    }
}

public enum DraftConnectionState
{
    Connected,
    
    Reconnecting,
    
    Disconnected
}