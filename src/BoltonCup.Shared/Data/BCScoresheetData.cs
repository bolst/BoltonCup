namespace BoltonCup.Shared.Data;


public partial class BCData
{
    public async Task<IEnumerable<BCGame>> GetActiveGamesAsync()
    {
        string sql = @"SELECT g.*,
                           h.name       AS hometeamname,
                           h.name_short AS hometeamnameshort,
                           h.logo_url   AS hometeamlogo,
                           a.name       AS awayteamname,
                           a.name_short AS awayteamnameshort,
                           a.logo_url   AS awayteamlogo
                        FROM game g
                                 LEFT OUTER JOIN team h ON g.home_team_id = h.id
                                 LEFT OUTER JOIN team a ON g.away_team_id = a.id
                        WHERE g.state = @State
                        ORDER BY g.date ASC";

        return await QueryDbAsync<BCGame>(sql, new { State = GameState.Active });
    }


    public async Task BeginRecordingGameAsync(int gameId)
    {
        string clearSql = $@"UPDATE game
                        SET state = '{GameState.PreGame}'
                            WHERE state = '{GameState.Active}'";

        await ExecuteSqlAsync(clearSql, new { GameId = gameId });
        
        string sql = $@"UPDATE game
                        SET state = '{GameState.Active}'
                            WHERE id = @GameId";

        await ExecuteSqlAsync(sql, new { GameId = gameId });
    }
    
    
    
    public async Task EndRecordingGameAsync(int gameId, bool complete = false)
    {
        string sql = @"UPDATE game
                        SET state = @State
                            WHERE id = @GameId";

        var state = complete ? GameState.Complete : GameState.PreGame;
        await ExecuteSqlAsync(sql, new { State = state, GameId = gameId });
    }
    
    
    
    public async Task AddGoalAsync(GoalEntry goal)
    {
        await ExecuteTransactionAsync(async () =>
        {
            string sql = @"INSERT INTO points(game_id, time, period, is_hometeam, tournament_id, scorer_id, assist1_player_id, assist2_player_id)
                            VALUES (@GameId, @Time, @Period, @IsHomeTeam, @TournamentId, @ScorerId, @Assist1Id, @Assist2Id)";

            await ExecuteSqlAsync(sql, new
            {
                GameId = goal.Game.id,
                Time = goal.Time,
                Period = goal.Period,
                IsHomeTeam = goal.Game.HomeTeamId == goal.Team.id,
                TournamentId = goal.Game.tournament_id,
                ScorerId = goal.Scorer.id,
                Assist1Id = goal.Assist1?.id,
                Assist2Id = goal.Assist2?.id,
            });

            await updateGameGoalsAsync(goal.Game.id);
        });
    }
    
    
    
    public async Task AddPenaltyAsync(PenaltyEntry penalty)
    {
        string sql = @"INSERT INTO penalties(time, period, duration_mins, infraction_name, notes, is_hometeam, game_id, tournament_id, player_id)
                        VALUES (@Time, @Period, @DurationMins, @Infraction, @Notes, @IsHomeTeam, @GameId, @TournamentId, @PlayerId)";

        await ExecuteSqlAsync(sql, new
        {
            Time = penalty.Time,
            Period = penalty.Period,
            DurationMins = penalty.DurationMins,
            Infraction = penalty.Infraction,
            Notes = penalty.Notes,
            IsHomeTeam = penalty.Game.HomeTeamId == penalty.Team.id,
            GameId = penalty.Game.id,
            TournamentId = penalty.Game.tournament_id,
            PlayerId = penalty.Player?.id
        });
    }

    public async Task RemoveGoalAsync(int goalId)
    {
        await ExecuteTransactionAsync(async () =>
        {
            var removal = await QueryDbSingleAsync<BCGoal>("DELETE FROM points WHERE id = @GoalId RETURNING *", new { GoalId = goalId });
            if (removal is null) return;
            
            await updateGameGoalsAsync(removal.game_id);
        });
    }
    
    public async Task RemovePenaltyAsync(int penaltyId)
        => await ExecuteSqlAsync(
            @"DELETE FROM penalties WHERE id = @PenaltyId", 
            new 
            {
                PenaltyId = penaltyId
            });
    
    
    public async Task<IEnumerable<BCInfraction>> GetInfractionsAsync()
        => await QueryDbAsync<BCInfraction>("SELECT * FROM infraction ORDER BY sort_key, name");
    
    
    #region helpers


    private async Task updateGameGoalsAsync(int gameId)
        => await ExecuteSqlAsync(
            @"WITH game_goals AS (SELECT (SELECT COUNT(DISTINCT id)
                                FROM points
                                WHERE game_id = @GameId
                                  AND is_hometeam = TRUE)  AS home_goals,
                           (SELECT COUNT(DISTINCT id)
                                FROM points
                                WHERE game_id = @GameId
                                  AND is_hometeam = FALSE) AS away_goals)
                    UPDATE game g
                    SET home_score = goals.home_goals,
                        away_score = goals.away_goals
                        FROM game_goals goals
                        WHERE g.id = @GameId", 
            new
            {
                GameId = gameId
            });


    #endregion


}