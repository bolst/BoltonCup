namespace BoltonCup.Shared.Data;


public interface ICacheService
{
    Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan cacheDuration);
    void Clear();
    void Clear(string cacheKey);
}