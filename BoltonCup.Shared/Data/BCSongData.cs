using SpotifyAPI.Web;

namespace BoltonCup.Shared.Data;




public partial class BCData
{

    public async Task UpdatePlayerSongAsync(int accountId, FullTrack song)
    {
        {
            string sql = @"INSERT INTO song(name, spotify_id, account_id, album_cover)
                            SELECT @Name, @Id, @AccountId, @AlbumCover
                                WHERE NOT EXISTS (SELECT * FROM song WHERE account_id = @AccountId)";

            await ExecuteSqlAsync(sql, new
            {
                Name = song.Name,
                Id = song.Id,
                AccountId = accountId,
                AlbumCover = song.Album.Images.FirstOrDefault()?.Url,
            });        
        }

        {
            string sql = @"UPDATE SONG SET 
                                name = @Name,
                                spotify_id = @Id,
                                album_cover = @AlbumCover
                            WHERE account_id = @AccountId";

            await ExecuteSqlAsync(sql, new
            {
                Name = song.Name,
                Id = song.Id,
                AccountId = accountId,
                AlbumCover = song.Album.Images.FirstOrDefault()?.Url,
            });
        }
    }

    public async Task SetBCPlaylistSongsAsync(IEnumerable<FullTrack> songs)
    {
        await ExecuteSqlAsync("DELETE FROM song WHERE account_id IS NULL");

        string sql = @"INSERT INTO song(spotify_id, name, album_cover) VALUES (@Id, @Name, @AlbumCover)";

        var parameters = songs.Select(x => new
        {
            Id = x.Id,
            Name = x.Name,
            AlbumCover = x.Album.Images.FirstOrDefault()?.Url,
        });
        
        await ExecuteSqlAsync(sql, parameters);
    }


    public async Task SetGamePlaylistAsync(int gameId, string playlistId)
    {
        string sql = @"UPDATE game
                        SET playlist_id = @PlaylistId
                            WHERE id = @GameId";
        
        await ExecuteSqlAsync(sql, new { GameId = gameId, PlaylistId = playlistId });
    }


    public async Task<IEnumerable<BCSong>> GetGameSongsAsync(int gameId)
    {
        string sql = @"WITH game_account_ids AS (
                        SELECT a.id
                          FROM account a
                           INNER JOIN players p ON p.account_id = a.id
                           INNER JOIN game g
                                      ON p.team_id IN (g.home_team_id, g.away_team_id) AND g.id = @GameId
                            )
                        SELECT *
                            FROM song
                            WHERE account_id IS NULL
                               OR account_id IN (SELECT * FROM game_account_ids)
                            ORDER BY account_id";

        return await QueryDbAsync<BCSong>(sql, new { GameId = gameId });
    }


    
    // idk if i like this style yet
    public async Task<int> GetSongOffsetAsync(string spotifyId) 
        => await QueryDbSingleAsync<int>(@"SELECT COALESCE(MAX(offset_seconds), 0)
                                            FROM song_offset
                                            WHERE spotify_id = @SpotifyId", 
            new 
            {
                SpotifyId = spotifyId
            });
    
        
}
