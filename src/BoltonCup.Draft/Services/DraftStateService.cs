using BoltonCup.Common;
using BoltonCup.Sdk;
using BoltonCup.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace BoltonCup.Draft.Services;


public class DraftStateService : IAsyncDisposable
{
    private readonly IBoltonCupApi _api;
    private readonly ILogger<DraftStateService> _logger;
    private readonly HubConnection _hubConnection;
    private readonly SemaphoreSlim _fetchLock;
    private readonly CancellationTokenSource _cts;
    
    private bool _disposed;
    private CancellationTokenSource? _pollCts;
    private int? _draftId;

    public DraftSingleDto? Draft { get; private set; }
    public DraftPickSingleDto? CurrentPick { get; private set; }
    public List<DraftRankingDto> PlayerRankings { get; private set; } = [];

    public DraftConnectionState ConnectionState { get; private set; } = DraftConnectionState.Disconnected;
    
    public bool CanEditDraft => Draft?.CanEditDraft ?? false;

    public event Action? OnStateChanged;

    public DraftStateService(
        IBoltonCupApi api,
        ILogger<DraftStateService> logger,
        IOptions<BoltonCupConfiguration> config
    )
    {
        _api = api;
        _logger = logger;
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
        
        // a completed draft will not have any updates
        if (Draft is null or { Status: DraftStatus.Completed })
            return;

        await EnsureConnectedAndJoinedAsync(draftId);
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


    private void HandleDraftUpdate(DraftUpdateEventDto eventDto)
    {
        Draft = eventDto.Draft;
        CurrentPick = eventDto.NextPick;
        NotifyStateChanged();
    }

    
    private async Task HandleDraftStatusChange(DraftStatus newStatus)
    {
        if (Draft is null) 
            return;
        Draft.Status = newStatus;
        NotifyStateChanged();
        
        if (newStatus == DraftStatus.Completed)
        {
            StopFallbackPoll();
            await StopHubAsync();
        }
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
            var draft = await _api.GetDraftByIdAsync(draftId, token);
            var currentPick = await _api.GetCurrentDraftPickAsync(draftId, token);
            var playerRankings = await _api.GetDraftPlayerRankingsAsync(draftId, size: 200, cancellationToken: token);

            Draft = draft;
            CurrentPick = currentPick;
            PlayerRankings = playerRankings.Items.ToList();
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
                NotifyStateChanged();
            }
        }
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