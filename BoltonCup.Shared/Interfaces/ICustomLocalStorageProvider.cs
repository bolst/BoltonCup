namespace BoltonCup.Shared.Data;

public interface ICustomLocalStorageProvider
{
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task SetAsync<T>(string key, T value);
    Task ClearAsync();
}