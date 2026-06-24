using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BoltonCup.Infrastructure.Services;

public sealed class SpotifyMusicSearchService : IMusicSearchService
{
    private const string TokenCacheKey = "spotify:token";
    private const string TokenEndpoint = "https://accounts.spotify.com/api/token";
    private const string SearchEndpoint = "https://api.spotify.com/v1/search";
    private const string PlaylistEndpoint = "https://api.spotify.com/v1/playlists";

    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly SpotifySettings _settings;
    private readonly IMemoryCache _cache;

    public SpotifyMusicSearchService(HttpClient httpClient, IOptions<SpotifySettings> settings, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _cache = cache;
    }

    public async Task<IReadOnlyList<MusicTrack>> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var token = await GetAccessTokenAsync(cancellationToken);

        var url = $"{SearchEndpoint}?q={Uri.EscapeDataString(query)}&type=track&limit={limit}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Spotify search failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken);
        var items = result?.Tracks?.Items;
        if (items is null)
            return [];

        return items.Select(t => new MusicTrack(
            Id: t.Id,
            Name: t.Name,
            Artist: t.Artists is { Count: > 0 } ? t.Artists[0].Name : string.Empty,
            AlbumArtUrl: t.Album?.Images is { Count: > 0 } images ? images[^1].Url : null
        )).ToList();
    }

    public async Task<IReadOnlyList<MusicTrack>> GetPlaylistTracksAsync(string playlistUrlOrId, CancellationToken cancellationToken = default)
    {
        var playlistId = ParsePlaylistId(playlistUrlOrId);
        if (string.IsNullOrWhiteSpace(playlistId))
            throw new ArgumentException($"Could not parse a Spotify playlist id from '{playlistUrlOrId}'.", nameof(playlistUrlOrId));

        var token = await GetAccessTokenAsync(cancellationToken);

        // Trim the payload to just the fields we need; page through until 'next' is null.
        var fields = Uri.EscapeDataString("items(track(id,name,artists(name),album(images))),next");
        var url = $"{PlaylistEndpoint}/{playlistId}/tracks?limit=100&fields={fields}";

        var tracks = new List<MusicTrack>();
        while (!string.IsNullOrEmpty(url))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Spotify playlist fetch failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
            }

            var page = await response.Content.ReadFromJsonAsync<PlaylistTracksPage>(cancellationToken);
            foreach (var item in page?.Items ?? [])
            {
                var t = item.Track;
                if (t is null || string.IsNullOrEmpty(t.Id))
                    continue; // local/removed tracks have no id and can't be matched

                tracks.Add(new MusicTrack(
                    Id: t.Id,
                    Name: t.Name,
                    Artist: t.Artists is { Count: > 0 } ? t.Artists[0].Name : string.Empty,
                    AlbumArtUrl: t.Album?.Images is { Count: > 0 } images ? images[^1].Url : null));
            }

            url = page?.Next;
        }

        return tracks;
    }

    // Accepts a raw id, an open.spotify.com URL, or a spotify:playlist:<id> URI.
    private static string? ParsePlaylistId(string input)
    {
        var value = input.Trim();
        if (value.Length == 0)
            return null;

        if (value.StartsWith("spotify:playlist:", StringComparison.OrdinalIgnoreCase))
            return value["spotify:playlist:".Length..].Split('?')[0];

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var idx = Array.FindIndex(segments, s => s.Equals("playlist", StringComparison.OrdinalIgnoreCase));
            return idx >= 0 && idx + 1 < segments.Length ? segments[idx + 1] : null;
        }

        // Otherwise assume the caller passed a bare playlist id.
        return value.Split('?')[0];
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
            return cached;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Re-check inside the lock: a concurrent caller may have populated the cache
            // while we waited, so only one request actually hits Spotify on a cold cache.
            if (_cache.TryGetValue(TokenCacheKey, out cached) && !string.IsNullOrEmpty(cached))
                return cached;

            return await FetchAndCacheTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> FetchAndCacheTokenAsync(CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Spotify token request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
            ?? throw new HttpRequestException("Spotify token response was empty.");

        _cache.Set(TokenCacheKey, token.AccessToken, TimeSpan.FromSeconds(Math.Max(60, token.ExpiresIn - 60)));
        return token.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record SearchResponse([property: JsonPropertyName("tracks")] TrackPage? Tracks);

    private sealed record TrackPage([property: JsonPropertyName("items")] List<TrackItem> Items);

    private sealed record TrackItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("artists")] List<Artist> Artists,
        [property: JsonPropertyName("album")] Album? Album);

    private sealed record PlaylistTracksPage(
        [property: JsonPropertyName("items")] List<PlaylistItem>? Items,
        [property: JsonPropertyName("next")] string? Next);

    private sealed record PlaylistItem([property: JsonPropertyName("track")] TrackItem? Track);

    private sealed record Artist([property: JsonPropertyName("name")] string Name);

    private sealed record Album([property: JsonPropertyName("images")] List<Image> Images);

    private sealed record Image([property: JsonPropertyName("url")] string Url);
}
