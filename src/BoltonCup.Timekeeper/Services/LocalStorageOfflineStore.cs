using System.Text.Json;
using BoltonCup.Sdk;
using Microsoft.JSInterop;

namespace BoltonCup.Timekeeper.Services;

public class LocalStorageOfflineStore : IOfflineStore
{
    private readonly IJSRuntime _js;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LocalStorageOfflineStore(IJSRuntime js) => _js = js;

    public async Task CacheGameAsync(GameSingleDto game)
    {
        var json = JsonSerializer.Serialize(game, Options);
        await _js.InvokeVoidAsync("localStorage.setItem", $"bc:game:{game.Id}", json);
    }

    public async Task<GameSingleDto?> TryLoadGameAsync(int gameId)
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", $"bc:game:{gameId}");
            return json is null ? null : JsonSerializer.Deserialize<GameSingleDto>(json, Options);
        }
        catch
        {
            return null;
        }
    }

    public async Task CacheRostersAsync(int gameId, List<PlayerDto> home, List<PlayerDto> away)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", $"bc:home:{gameId}", JsonSerializer.Serialize(home, Options));
        await _js.InvokeVoidAsync("localStorage.setItem", $"bc:away:{gameId}", JsonSerializer.Serialize(away, Options));
    }

    public async Task<(List<PlayerDto> Home, List<PlayerDto> Away)> TryLoadRostersAsync(int gameId)
    {
        try
        {
            var homeJson = await _js.InvokeAsync<string?>("localStorage.getItem", $"bc:home:{gameId}");
            var awayJson = await _js.InvokeAsync<string?>("localStorage.getItem", $"bc:away:{gameId}");
            var home = homeJson is not null ? JsonSerializer.Deserialize<List<PlayerDto>>(homeJson, Options) ?? [] : new List<PlayerDto>();
            var away = awayJson is not null ? JsonSerializer.Deserialize<List<PlayerDto>>(awayJson, Options) ?? [] : new List<PlayerDto>();
            return (home, away);
        }
        catch
        {
            return (new List<PlayerDto>(), new List<PlayerDto>());
        }
    }

    public async Task EnqueueAsync(int gameId, OfflineEventRecord record)
    {
        var queue = await GetQueueAsync(gameId);
        queue.Add(record);
        await SaveQueueAsync(gameId, queue);
    }

    public async Task<List<OfflineEventRecord>> GetQueueAsync(int gameId)
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", $"bc:queue:{gameId}");
            return json is null ? new List<OfflineEventRecord>() : JsonSerializer.Deserialize<List<OfflineEventRecord>>(json, Options) ?? new List<OfflineEventRecord>();
        }
        catch
        {
            return new List<OfflineEventRecord>();
        }
    }

    public async Task RemoveFromQueueAsync(int gameId, Guid id)
    {
        var queue = await GetQueueAsync(gameId);
        queue.RemoveAll(e => e.Id == id);
        await SaveQueueAsync(gameId, queue);
    }

    public async Task<int> GetPendingCountAsync(int gameId)
        => (await GetQueueAsync(gameId)).Count;

    private async Task SaveQueueAsync(int gameId, List<OfflineEventRecord> queue)
    {
        var json = JsonSerializer.Serialize(queue, Options);
        await _js.InvokeVoidAsync("localStorage.setItem", $"bc:queue:{gameId}", json);
    }
}
