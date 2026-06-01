using BoltonCup.Sdk;

namespace BoltonCup.Timekeeper.Services;

public interface IOfflineStore
{
    Task CacheGameAsync(GameSingleDto game);
    Task<GameSingleDto?> TryLoadGameAsync(int gameId);
    Task CacheRostersAsync(int gameId, List<PlayerDto> home, List<PlayerDto> away);
    Task<(List<PlayerDto> Home, List<PlayerDto> Away)> TryLoadRostersAsync(int gameId);
    Task EnqueueAsync(int gameId, OfflineEventRecord record);
    Task<List<OfflineEventRecord>> GetQueueAsync(int gameId);
    Task RemoveFromQueueAsync(int gameId, Guid id);
    Task<int> GetPendingCountAsync(int gameId);
}
