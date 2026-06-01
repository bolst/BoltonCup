using System.Text.Json;
using System.Text.Json.Serialization;
using BoltonCup.Sdk;
using Microsoft.JSInterop;

namespace BoltonCup.Timekeeper.Services;

public class SyncService : IAsyncDisposable
{
    private readonly IBoltonCupApi _api;
    private readonly IOfflineStore _store;
    private readonly IJSRuntime _js;
    private readonly ILogger<SyncService> _logger;
    private readonly CancellationTokenSource _cts = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool IsOnline { get; private set; } = true;
    public int PendingCount { get; private set; }
    public DateTime? LastSyncAt { get; private set; }

    public event Action? OnStatusChanged;
    public event Action? OnSyncCompleted;

    private int? _activeGameId;

    public SyncService(IBoltonCupApi api, IOfflineStore store, IJSRuntime js, ILogger<SyncService> logger)
    {
        _api = api;
        _store = store;
        _js = js;
        _logger = logger;
    }

    public void SetActiveGame(int? gameId)
    {
        _activeGameId = gameId;
        PendingCount = 0;
        OnStatusChanged?.Invoke();
    }

    public Task StartAsync()
    {
        _ = RunLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                await TickAsync();
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task TickAsync()
    {
        try
        {
            IsOnline = await _js.InvokeAsync<bool>("eval", "navigator.onLine");

            if (IsOnline && _activeGameId.HasValue)
            {
                await ProcessQueueAsync(_activeGameId.Value);
            }

            PendingCount = _activeGameId.HasValue
                ? await _store.GetPendingCountAsync(_activeGameId.Value)
                : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sync tick error");
        }
        finally
        {
            OnStatusChanged?.Invoke();
        }
    }

    private async Task ProcessQueueAsync(int gameId)
    {
        var queue = await _store.GetQueueAsync(gameId);
        if (queue.Count == 0) return;

        var anySynced = false;
        foreach (var record in queue)
        {
            try
            {
                await DispatchAsync(gameId, record);
                await _store.RemoveFromQueueAsync(gameId, record.Id);
                anySynced = true;
                LastSyncAt = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync event {EventType} {Id}", record.EventType, record.Id);
                break;
            }
        }

        if (anySynced)
        {
            OnSyncCompleted?.Invoke();
        }
    }

    private async Task DispatchAsync(int gameId, OfflineEventRecord record)
    {
        switch (record.EventType)
        {
            case "AddGoal":
                await _api.AddGoalAsync(gameId, Deserialize<CreateGoalRequest>(record.PayloadJson));
                break;
            case "DeleteGoal":
                await _api.DeleteGoalAsync(gameId, Deserialize<DeleteIdPayload>(record.PayloadJson).Id);
                break;
            case "AddPenalty":
                await _api.AddPenaltyAsync(gameId, Deserialize<CreatePenaltyRequest>(record.PayloadJson));
                break;
            case "DeletePenalty":
                await _api.DeletePenaltyAsync(gameId, Deserialize<DeleteIdPayload>(record.PayloadJson).Id);
                break;
            case "GameState":
                await _api.UpdateGameStateAsync(gameId, Deserialize<UpdateGameStateRequest>(record.PayloadJson));
                break;
            case "Stars":
                await _api.SetGameStarsAsync(gameId, Deserialize<SetGameStarsRequest>(record.PayloadJson));
                break;
            default:
                _logger.LogWarning("Unknown event type: {EventType}", record.EventType);
                break;
        }
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options)
        ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}");

    public sealed record DeleteIdPayload
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
