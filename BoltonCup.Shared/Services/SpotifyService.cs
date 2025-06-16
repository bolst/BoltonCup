using SpotifyAPI.Web;

namespace BoltonCup.Shared.Data;

public class SpotifyService(string clientId, string secret)
{
    public async Task<SearchResponse> SearchSongs(string song, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = SpotifyClientConfig.CreateDefault();
            var response = await new OAuthClient(config).RequestToken(
                new ClientCredentialsRequest(clientId, secret), cancellationToken);

            var spotify = new SpotifyClient(response);
            
            var request = new SearchRequest(SearchRequest.Types.Track, $"{song}");

            return await spotify.Search.Item(request, cancel: cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in Spotify Search: {e.Message}");
            return new();
        }
    }
    
}