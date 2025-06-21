namespace BoltonCup.Shared.Data;

public partial class BCData
{
    public async Task<BCRefreshToken?> GetRefreshToken(Guid localId)
    {
        string sql = @"SELECT *
                        FROM refresh_token
                        WHERE local_id = @LocalId
                        ORDER BY created_at DESC
                        LIMIT 1";
        
        return await QueryDbSingleAsync<BCRefreshToken>(sql, new { LocalId = localId });
    }


    public async Task UpdateRefreshToken(Guid localId, string token)
    {
        string sql = @"INSERT INTO refresh_token(token, local_id)
                        VALUES (@Token, @LocalId)";
        
        await ExecuteSqlAsync(sql, new { LocalId = localId, Token = token });
    }
}
