using SpotifyAPI.Web;

namespace BoltonCup.Shared.Data;




public partial class BCData
{

    public async Task SetBCPlaylistSongsAsync(IEnumerable<FullTrack> songs)
    {
        await ExecuteSqlAsync("DELETE FROM song");

        string sql = @"INSERT INTO song(spotify_id, name, album_cover) VALUES (@Id, @Name, @AlbumCover)";

        var parameters = songs.Select(x => new
        {
            Id = x.Id,
            Name = x.Name,
            AlbumCover = x.Album.Images.FirstOrDefault()?.Url,
        });
        
        await ExecuteSqlAsync(sql, parameters);
    }

    public async Task<IEnumerable<BCSong>> GetSongQueueAsync()
    {
        string sql = $@"SELECT *
                        FROM song
                        WHERE state = '{SongState.Queued}'
                        ORDER BY last_played NULLS FIRST, id";

        return await QueryDbAsync<BCSong>(sql);
    }
    
    
    
    public async Task<BCSong?> GetCurrentSongAsync()
    {
        string sql = $@"SELECT *
                        FROM song
                        WHERE state IN ('{SongState.Playing}', '{SongState.Paused}')
                        LIMIT 1";

        return await QueryDbSingleAsync<BCSong>(sql);
    }
    
    
    
    public async Task PauseSongAsync(BCSong song)
    {
        string sql = $@"UPDATE song
                        SET state = '{SongState.Paused}'
                            WHERE id = @Id";
        
        await ExecuteSqlAsync(sql, song);
    }
    
    
    
    public async Task PlaySongAsync(BCSong song)
    {
        await ExecuteSqlAsync($"UPDATE song SET state = '{SongState.Queued}'");
        
        string sql = $@"UPDATE song
                        SET state = '{SongState.Playing}', last_played = now() AT TIME ZONE 'UTC'
                            WHERE id = @Id";

        await ExecuteSqlAsync(sql, song);
    }



    public async Task<BCSong?> GetNextSongAsync()
    {
        string sql = $@"SELECT *
                        FROM song
                        WHERE state = '{SongState.Queued}'
                        ORDER BY last_played NULLS FIRST, id
                        LIMIT 1";

        return await QueryDbSingleAsync<BCSong>(sql);
    }



    public async Task<BCSong?> SkipToNextSongAsync()
    {
        // put all songs back in queue
        await ExecuteSqlAsync($"UPDATE song SET state = '{SongState.Queued}'");

        var nextSong = await GetNextSongAsync();
        if (nextSong is null) return null;
        
        // update next song to playing
        string sql = $@"UPDATE song
                        SET state = '{SongState.Playing}', last_played = now() AT TIME ZONE 'UTC'
                            WHERE id = @Id
                        RETURNING *";

        return await QueryDbSingleAsync<BCSong>(sql, nextSong);
    }
}
