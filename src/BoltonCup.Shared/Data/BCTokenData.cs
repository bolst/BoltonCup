namespace BoltonCup.Shared.Data;

public partial class BCData
{
    public async Task<BCRefreshToken?> GetRefreshTokenAsync(Guid localId, TokenProvider provider)
    {
        await DeleteOldTokensAsync();
        
        string sql = @"SELECT *
                        FROM auth_token
                        WHERE local_id = @LocalId
                            AND provider = @Provider
                        ORDER BY created_at DESC
                        LIMIT 1";
        
        return await QueryDbSingleAsync<BCRefreshToken>(sql, new { LocalId = localId, Provider = provider.ToDescriptionString() });
    }


    public async Task UpdateRefreshTokenAsync(BCRefreshToken refreshToken)
    {
        string sql = @"INSERT INTO auth_token(refresh, local_id, provider, access)
                        VALUES (@refresh, @local_id, @provider, @access)";
        
        await ExecuteSqlAsync(sql, refreshToken);
    }


    private async Task DeleteOldTokensAsync()
        => await ExecuteSqlAsync(@"DELETE
                                            FROM auth_token
                                            WHERE created_at < NOW() - INTERVAL '3' DAY");

}
