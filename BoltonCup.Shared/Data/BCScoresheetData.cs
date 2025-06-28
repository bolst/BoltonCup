namespace BoltonCup.Shared.Data;




public partial class BCData
{
    public async Task AddGoalAsync(GoalEntry goal)
    {
        string sql = @"INSERT INTO points(game_id, time, period, is_hometeam, tournament_id, scorer_id, assist1_player_id, assist2_player_id)
                        VALUES (@GameId, @Time, @Period, @IsHomeTeam, @TournamentId, @ScorerId, @Assist1Id, @Assist2Id)";

        await ExecuteSqlAsync(sql, new
        {
            GameId = goal.Game.id,
            Time = DateTime.Now,
            Period = 1,
            IsHomeTeam = goal.Game.home_team_id == goal.Team.id,
            TournamentId = goal.Game.tournament_id,
            ScorerId = goal.Scorer.id,
            Assist1Id = goal.Assist1?.id,
            Assist2Id = goal.Assist2?.id,
        });
    }
    
    
    
    public async Task AddPenaltyAsync(PenaltyEntry penalty)
    {
        string sql = @"INSERT INTO penalties(time, period, duration_mins, infraction_name, is_hometeam, game_id, tournament_id, player_id)
                        VALUES (@Time, @Period, @DurationMins, @Infraction, @IsHomeTeam, @GameId, @TournamentId, @PlayerId)";

        await ExecuteSqlAsync(sql, new
        {
            Time = DateTime.Now,
            Period = 1,
            DurationMins = penalty.DurationMins,
            Infraction = penalty.Infraction,
            IsHomeTeam = penalty.Game.home_team_id == penalty.Team.id,
            GameId = penalty.Game.id,
            TournamentId = penalty.Game.tournament_id,
            PlayerId = penalty.Player?.id
        });
    }


    public async Task RemoveGoalAsync(int goalId)
        => await ExecuteSqlAsync(
            @"DELETE FROM points WHERE id = @GoalId", 
            new 
            {
                GoalId = goalId
            });
    
    
    public async Task RemovePenaltyAsync(int penaltyId)
        => await ExecuteSqlAsync(
            @"DELETE FROM penalties WHERE id = @PenaltyId", 
            new 
            {
                PenaltyId = penaltyId
            });
    
    
}