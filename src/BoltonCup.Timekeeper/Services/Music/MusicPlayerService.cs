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

    // One-shot goal song: plays over the single <audio> element, then stops (no playlist advance).
    private bool _oneShot;
    private PlaylistTrackDto? _oneShotTrack;

    public IReadOnlyList<PlaylistTrackDto> Playlist { get; private set; } = [];
    public int CurrentIndex { get; private set; } = -1;
    public bool IsPlaying { get; private set; }
    public double PositionSec { get; private set; }
    public double DurationSec { get; private set; }

    public PlaylistTrackDto? CurrentTrack =>
        _oneShotTrack
        ?? (CurrentIndex >= 0 && CurrentIndex < Playlist.Count ? Playlist[CurrentIndex] : null);

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

        // Leaving any goal-song one-shot: clear its state so CurrentTrack reflects the playlist track
        // (and the next natural 'ended' advances the playlist instead of stopping).
        _oneShot = false;
        _oneShotTrack = null;

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

    /// <summary>
    /// Plays a team's goal song as a one-shot over the current audio element. When it ends (or fails)
    /// playback stops — it never advances the playlist. No-op if the module isn't ready, the key is
    /// empty, or the track can't be resolved (offline &amp; uncached).
    /// </summary>
    public async Task<GoalSongResult> PlayGoalSongAsync(string fileKey, int offsetSeconds, string? title = null)
    {
        if (_module is null)
        {
            return GoalSongResult.NotReady;
        }
        if (string.IsNullOrEmpty(fileKey))
        {
            return GoalSongResult.NoSong;
        }

        var url = await _cache.GetPlayableUrlAsync(fileKey);
        if (url is null)
        {
            // Offline and not cached — leave playlist state untouched and play nothing.
            return GoalSongResult.Unresolved;
        }

        await RevokeCurrentAsync();

        _oneShot = true;
        _oneShotTrack = new PlaylistTrackDto { FileKey = fileKey, Title = title ?? "Goal!", OffsetSeconds = offsetSeconds };

        if (url.StartsWith("blob:"))
        {
            _currentObjectUrl = url;
        }

        DurationSec = 0;
        PositionSec = offsetSeconds;
        await _module.InvokeVoidAsync("load", url, offsetSeconds);
        IsPlaying = await _module.InvokeAsync<bool>("play");
        Notify();
        return IsPlaying ? GoalSongResult.Playing : GoalSongResult.Blocked;
    }

    public enum GoalSongResult
    {
        Playing,     // started successfully
        NoSong,      // no file key supplied (team has none, or the API didn't send one)
        Unresolved,  // couldn't resolve a URL (offline and not cached)
        Blocked,     // load/play attempted but the browser refused (autoplay policy)
        NotReady,    // audio module not initialized yet
    }

    private async Task StopAfterOneShotAsync()
    {
        _oneShot = false;
        _oneShotTrack = null;
        await RevokeCurrentAsync();
        CurrentIndex = -1; // stop cleanly; a later Play starts the playlist from the top
        IsPlaying = false;
        PositionSec = 0;
        DurationSec = 0;
        Notify();
    }

    [JSInvokable]
    public async Task OnEnded()
    {
        if (_oneShot)
        {
            await StopAfterOneShotAsync();
            return;
        }
        await NextAsync();
    }

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
        if (_oneShot)
        {
            // A goal song failed to play — stop cleanly rather than hijacking the playlist.
            await StopAfterOneShotAsync();
            return;
        }
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
