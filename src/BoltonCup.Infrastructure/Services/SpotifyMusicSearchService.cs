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

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
            return cached;

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

    private sealed record Artist([property: JsonPropertyName("name")] string Name);

    private sealed record Album([property: JsonPropertyName("images")] List<Image> Images);

    private sealed record Image([property: JsonPropertyName("url")] string Url);
}
