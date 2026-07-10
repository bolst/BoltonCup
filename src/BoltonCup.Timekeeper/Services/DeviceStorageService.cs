using Microsoft.JSInterop;

namespace BoltonCup.Timekeeper.Services;

/// <summary>
/// Clears locally-persisted Timekeeper data on this device (localStorage and all IndexedDB databases,
/// including the downloaded music cache) via storage.js.
/// </summary>
public sealed class DeviceStorageService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public DeviceStorageService(IJSRuntime js)
    {
        _js = js;
    }

    private async Task<IJSObjectReference> ModuleAsync()
        // Query string dodges Blazor's fingerprint import map so the dev server serves it.
        => _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "/js/storage.js?v=1.0.0");

    /// <summary>Wipes localStorage and every IndexedDB database for this origin.</summary>
    public async Task ClearAllAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("clearAll");

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
