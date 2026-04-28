using BoltonCup.Common;
using BoltonCup.Sdk;
using BoltonCup.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace BoltonCup.Draft.Services;


public class DraftStateService : IAsyncDisposable
{
    private readonly IBoltonCupApi _api;
    private readonly HubConnection _hubConnection;
    private int? _draftId;

    public DraftSingleDto? Draft { get; private set; }
    public DraftPickSingleDto? CurrentPick { get; private set; }
    public List<DraftRankingDto> PlayerRankings { get; private set; } = [];

    public event Action? OnStateChanged;

    public DraftStateService(
        IBoltonCupApi api,
        IOptions<BoltonCupConfiguration> config
    )
    {
        _api = api;

        var hubUrl = $"{config.Value.ApiBaseUrl.TrimEnd('/')}{Hubs.Draft}";
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<DraftUpdateEventDto>(HubEvents.Draft.OnDraftUpdate, HandleDraftUpdate);
        _hubConnection.On<DraftStatus>(HubEvents.Draft.OnDraftStatusChange, HandleDraftStatusChange);
        _hubConnection.On<DraftPickMadeEventDto>(HubEvents.Draft.OnPickMade, HandlePickMade);
    }

    public async Task InitializeAsync(int draftId)
    {
        _draftId = draftId;
        
        Draft = await _api.GetDraftByIdAsync(draftId);
        CurrentPick = await _api.GetCurrentDraftPickAsync(draftId);
        PlayerRankings = (await _api.GetDraftPlayerRankingsAsync(draftId)).Items.ToList();

        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }

        await _hubConnection.InvokeAsync("JoinLiveDraft", draftId);
        
        NotifyStateChanged();
    }


    private void HandleDraftUpdate(DraftUpdateEventDto eventDto)
    {
        Draft = eventDto.Draft;
        CurrentPick = eventDto.NextPick;
        NotifyStateChanged();
    }

    
    private void HandleDraftStatusChange(DraftStatus newStatus)
    {
        if (Draft is null) 
            return;
        Draft.Status = newStatus;
        NotifyStateChanged();
    }

    
    private void HandlePickMade(DraftPickMadeEventDto eventDto)
    {
        var existingPick = Draft?.DraftPicks
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

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected && _draftId.HasValue)
        {
            await _hubConnection.InvokeAsync("LeaveLiveDraft", _draftId.Value);
        }

        await _hubConnection.DisposeAsync();
    }
}