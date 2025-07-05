using Blazored.LocalStorage;
using BoltonCup.Shared.Data;

namespace BoltonCup.Scoresheet.Data;

public class CustomLocalStorageProvider : ICustomLocalStorageProvider
{
    private readonly ILocalStorageService _localStorageService;

    public CustomLocalStorageProvider(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }
    
    public async Task<T?> GetAsync<T>(string key) => await _localStorageService.GetItemAsync<T>(key);
    
    public async Task RemoveAsync(string key) => await _localStorageService.RemoveItemAsync(key);
    
    public async Task SetAsync<T>(string key, T value) => await _localStorageService.SetItemAsync(key, value);

    public async Task ClearAsync() => await _localStorageService.ClearAsync();
    
}