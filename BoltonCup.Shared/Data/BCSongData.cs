using SpotifyAPI.Web;

namespace BoltonCup.Shared.Data;




public partial class BCData
{
    // TODO: get players in game and their requested songs in order before the preset songs
    public async Task<IEnumerable<BCSong>> GetGameSongsAsync(int gameId)
    {
        string sql = @"SELECT a.songrequest AS name, a.songrequestid AS spotify_id, a.songlastplayed as last_played
                            FROM account a
                                     INNER JOIN game g ON g.id = @GameId
                                     INNER JOIN players p ON p.account_id = a.id AND p.team_id IN (g.home_team_id, g.away_team_id)
                            WHERE a.songrequest IS NOT NULL
                        UNION
                        SELECT name, spotify_id, last_played
                            FROM song";
        return await QueryDbAsync<BCSong>(sql, new { GameId = gameId });
    }



    public async Task<IEnumerable<BCSong>> GetBCPlaylistSongsAsync()
    {
        string sql = @"SELECT name, spotify_id, last_played
                        FROM song
                        ORDER BY last_played NULLS FIRST";
        return await QueryDbAsync<BCSong>(sql);
    }


    public async Task SetBCPlaylistSongsAsync(IEnumerable<FullTrack> songs)
    {
        await ExecuteSqlAsync("DELETE FROM song");

        string sql = @"INSERT INTO song(spotify_id, name) VALUES (@Id, @Name)";
        
        await ExecuteSqlAsync(sql, songs);
    }


    public async Task SetSongAsPlayedAsync(BCSong song)
    {
        string sql = @"UPDATE song
                        SET last_played = NOW() AT TIME ZONE 'utc'
                        where spotify_id = @spotify_id";
        
        await ExecuteSqlAsync(sql, song);
    }


    public async Task<BCSong?> GetNextSongAsync()
    {
        string sql = @"SELECT *
                        FROM song
                        ORDER BY last_played NULLS FIRST
                        LIMIT 1";
        
        return await QueryDbSingleAsync<BCSong>(sql);
    }
}
