using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BoltonCup.Timekeeper.Services.Music;

/// <summary>
/// Holds the current game playlist and playback state, and drives the hidden &lt;audio&gt; element through
/// musicPlayer.js. Owns the single current blob object URL and the DotNetObjectReference for media callbacks.
/// </summary>
public sealed class MusicPlayerService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly MusicCacheService _cache;

    private IJSObjectReference? _module;
    private DotNetObjectReference<MusicPlayerService>? _selfRef;
    private string? _currentObjectUrl;

    public IReadOnlyList<PlaylistTrackDto> Playlist { get; private set; } = [];
    public int CurrentIndex { get; private set; } = -1;
    public bool IsPlaying { get; private set; }
    public double PositionSec { get; private set; }
    public double DurationSec { get; private set; }

    public PlaylistTrackDto? CurrentTrack =>
        CurrentIndex >= 0 && CurrentIndex < Playlist.Count ? Playlist[CurrentIndex] : null;

    public event Action? OnStateChanged;

    public MusicPlayerService(IJSRuntime js, MusicCacheService cache)
    {
        _js = js;
        _cache = cache;
    }

    public async Task InitializeAsync(ElementReference audioEl)
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "/js/musicPlayer.js?v=1");
        _selfRef ??= DotNetObjectReference.Create(this);
        await _module.InvokeVoidAsync("init", audioEl, _selfRef);
    }

    public void SetPlaylist(IReadOnlyList<PlaylistTrackDto> tracks)
    {
        Playlist = tracks;
        CurrentIndex = -1;
        PositionSec = 0;
        DurationSec = 0;
        Notify();
    }

    public Task PlayAsync() => CurrentIndex < 0 ? PlayIndexAsync(0) : ResumeAsync();
    public Task TogglePlayPauseAsync() => IsPlaying ? PauseAsync() : PlayAsync();
    public Task NextAsync() => CurrentIndex + 1 < Playlist.Count ? PlayIndexAsync(CurrentIndex + 1) : StopAsync();
    public Task PrevAsync() => CurrentIndex > 0 ? PlayIndexAsync(CurrentIndex - 1) : Task.CompletedTask;

    public async Task PlayIndexAsync(int index)
    {
        if (_module is null || index < 0 || index >= Playlist.Count)
        {
            return;
        }

        await RevokeCurrentAsync();

        CurrentIndex = index;
        var track = Playlist[index];
        DurationSec = (track.DurationMs ?? 0) / 1000.0;
        var offsetSec = track.OffsetSeconds ?? 0;
        PositionSec = offsetSec;

        var url = await _cache.GetPlayableUrlAsync(track.FileKey);
        if (url is null)
        {
            // Offline and not cached — skip to the next playable track.
            Notify();
            await NextAsync();
            return;
        }

        if (url.StartsWith("blob:"))
        {
            _currentObjectUrl = url;
        }

        await _module.InvokeVoidAsync("load", url, offsetSec);
        IsPlaying = await _module.InvokeAsync<bool>("play");
        Notify();
    }

    public async Task PauseAsync()
    {
        if (_module is null)
        {
            return;
        }
        await _module.InvokeVoidAsync("pause");
        IsPlaying = false;
        Notify();
    }

    public async Task SeekAsync(double sec)
    {
        if (_module is null)
        {
            return;
        }
        await _module.InvokeVoidAsync("seek", sec);
        PositionSec = sec;
        Notify();
    }

    private async Task ResumeAsync()
    {
        if (_module is null)
        {
            return;
        }
        IsPlaying = await _module.InvokeAsync<bool>("play");
        Notify();
    }

    private Task StopAsync()
    {
        IsPlaying = false;
        Notify();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnEnded() => await NextAsync();

    [JSInvokable]
    public Task OnTimeUpdate(double currentSec, double durationSec)
    {
        PositionSec = currentSec;
        if (durationSec > 0)
        {
            DurationSec = durationSec;
        }
        Notify();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnLoadedMetadata(double durationSec)
    {
        if (durationSec > 0)
        {
            DurationSec = durationSec;
        }
        Notify();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnPlaybackError(string message)
    {
        IsPlaying = false;
        Notify();
        // A corrupt/unplayable track — advance so the operator isn't stuck.
        await NextAsync();
    }

    private async Task RevokeCurrentAsync()
    {
        if (_currentObjectUrl is not null)
        {
            try
            {
                await _cache.RevokeAsync(_currentObjectUrl);
            } 
            catch (JSDisconnectedException) { }
            _currentObjectUrl = null;
        }
    }

    private void Notify() => OnStateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException) { }
        await RevokeCurrentAsync();
        _selfRef?.Dispose();
    }
}
