using Dapper;

namespace BoltonCup.Shared.Data;


public partial class BCData
{

    public async Task<BCDraft?> GetTournamentDraftAsync(int tournamentId)
    {
        string sql = @"SELECT *
                        FROM
	                        draftinfo
                        WHERE
	                        tournamentid = @TournamentId";
        return await QueryDbSingleAsync<BCDraft>(sql, new { TournamentId = tournamentId });
    }
    
    
    public async Task<BCDraftPick?> GetMostRecentDraftPickAsync(int draftId)
    {
        string sql = @"WITH max_round AS (SELECT MAX(round)
                       FROM draftpick
                       WHERE draft_id = @DraftId),
                             max_round_picks AS (SELECT * FROM draftpick WHERE round IN (SELECT * FROM max_round) AND draft_id = @DraftId)
                        SELECT d.*
                            FROM draftpick d
                                     INNER JOIN max_round_picks m ON d.id = m.id AND d.pick = (SELECT MAX(pick) FROM max_round_picks)";
        
        return await QueryDbSingleAsync<BCDraftPick>(sql, new { DraftId = draftId });
    }

    
    public async Task<BCTeam?> GetTeamByDraftOrderAsync(int draftId, int order)
    {
        string sql = @"select
                          t.*
                        from
                          team t
                          inner join draftorder o on t.id = o.team_id
                        where
                          o.order = @Order
                          and o.draft_id = @DraftId";
        
        return await QueryDbSingleAsync<BCTeam>(sql, new { DraftId = draftId, Order = order });
    }
    
    
    public async Task<IEnumerable<PlayerProfile>> GetDraftAvailableTournamentPlayersAsync(int tournamentId)
    {
        string sql = @"SELECT a.*, p.*, JSON_OBJECT_AGG(COALESCE(g.date, '2001-01-01'), COALESCE(b.availability, 'null')) AS availabilities
                        FROM players p
                                 LEFT OUTER JOIN account a ON p.account_id = a.id AND a.isactive = TRUE
                                 LEFT OUTER JOIN availability b ON b.account_id = a.id
                                 LEFT OUTER JOIN game g ON g.id = b.game_id
                        WHERE p.tournament_id = @TournamentId
                          AND p.team_id IS NULL
                        GROUP BY p.id, a.id
                        ORDER BY birthday, p.name";
        return await QueryDbAsync<PlayerProfile>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<BCDraftOrder>> GetDraftOrderAsync(int draftId)
    {
        string sql = @"SELECT *
                        FROM draftorder
                        WHERE draft_id = @DraftId";
        
        return await QueryDbAsync<BCDraftOrder>(sql, new { DraftId = draftId });
    }

    public async Task DraftPlayerAsync(PlayerProfile player, BCTeam team, BCDraftPick draftPick)
    {
        
        string playerSql = @"UPDATE players
                        SET team_id = @TeamId
                            WHERE id = @PlayerId
                            AND tournament_id = @TournamentId";
        await ExecuteSqlAsync(playerSql, new { TeamId = team.id, PlayerId = player.id, TournamentId = player.tournament_id });
        
        draftPick.player_id = player.id;
        string draftSql = @"INSERT INTO draftpick (draft_id, round, pick, player_id)
                                VALUES (@draft_id, @round, @pick, @player_id)";
        await ExecuteSqlAsync(draftSql, draftPick);
    }

    public async Task<IEnumerable<BCDraftPickDetail>> GetDraftPicksAsync(int draftId)
    {
        string sql = @"SELECT dp.*,
                               p.name,
                               p.dob        AS birthday,
                               p.team_id    AS teamid,
                               p.position,
                               a.profilepicture,
                               t.name       AS teamname,
                               t.name_short AS teamnameshort,
                               t.id         AS teamid,
                               t.logo_url   AS teamlogo,
                               t.primary_color_hex AS PrimaryColorHex,
                               t.secondary_color_hex AS SecondaryColorHex,
                               t.tertiary_color_hex AS TertiaryColorHex
                            FROM draftpick dp
                                     INNER JOIN players p ON p.id = dp.player_id AND dp.draft_id = @DraftId
                                     INNER JOIN team t ON t.id = p.team_id
                                     LEFT OUTER JOIN account a ON a.id = p.account_id
                            ORDER BY round, pick";
        
        return await QueryDbAsync<BCDraftPickDetail>(sql, new { DraftId = draftId });
    }

    public async Task ResetDraftAsync(int draftId)
    {
        
        // delete all draft picks in draft
        string draftSql = @"DELETE
                                FROM draftpick
                                WHERE draft_id = @DraftId
                                RETURNING player_id";
        var deletedPicks = await QueryDbAsync<int>(draftSql, new { DraftId = draftId });
        
        // set all player teams to null
        {
            string sql = @"UPDATE players
                                    SET team_id = NULL
                                        WHERE tournament_id = (SELECT tournament_id FROM draft WHERE id = @DraftId)
                                        AND id = ANY(@PlayerIds)";
            await ExecuteSqlAsync(sql, new
            {
                DraftId = draftId,
                PlayerIds = deletedPicks.ToList(),
            });
        }
    }


    public async Task UpdateDraftStateAsync(int draftId, DraftState state)
    {
        string sql = @"UPDATE draft
                        SET state = @State
                        WHERE id = @DraftId";
        
        await ExecuteSqlAsync(sql, new { DraftId = draftId, State = state.ToDescriptionString() });
    }



    public async Task<IEnumerable<DraftRanking>> GetDraftRankingsAsync(int tournamentId, int accountId)
    {
        string sql = @"SELECT p.id AS playerid,
                               p.name AS playername,
                               p.account_id AS accountid,
                               p.team_id AS teamid,
                               t.name AS teamname,
                               t.logo_url AS teamlogo,
                               p.position,
                               COALESCE(r.rank, ROW_NUMBER() OVER ( PARTITION BY p.position
                                   ORDER BY RANDOM())) AS rank
                        FROM players p
                                 INNER JOIN account a
                                            ON p.account_id = a.id AND a.isactive = TRUE AND a.payed = TRUE AND p.tournament_id = @TournamentId
                                 LEFT OUTER JOIN draft_ranking r ON r.player_id = p.id AND r.account_id = @AccountId
                                 LEFT OUTER JOIN team t ON t.id = p.team_id
                        WHERE a.id NOT IN (SELECT gm_account_id FROM team WHERE tournament_id = @TournamentId)
                        ORDER BY CASE WHEN p.position = 'forward' THEN 1 WHEN p.position = 'defense' THEN 2 ELSE 3 END, rank";
        
        return await QueryDbAsync<DraftRanking>(sql, new { TournamentId = tournamentId, AccountId = accountId });
    }



    public async Task UpdateDraftRankingAsync(int draftId, int accountId, IEnumerable<DraftRanking> rankings)
    {
        await ExecuteTransactionAsync(async () =>
        {
            {
                string sql = @"DELETE FROM draft_ranking WHERE draft_id = @DraftId AND account_id = @AccountId";
                
                await ExecuteSqlAsync(sql, new { DraftId = draftId, AccountId = accountId });
            }

            {
                string sql = @"INSERT INTO draft_ranking(account_id, player_id, rank, draft_id)
                                    VALUES(@AccountId, @PlayerId, @Rank, @DraftId)";

                var parameters = rankings.Select(x => new DynamicParameters(new
                {
                    AccountId = accountId,
                    PlayerId = x.PlayerId,
                    Rank = x.Rank,
                    DraftId = draftId,
                }));

                await ExecuteSqlAsync(sql, parameters);
            }
        });
    }
}