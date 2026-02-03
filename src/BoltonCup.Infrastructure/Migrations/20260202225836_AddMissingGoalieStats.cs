using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingGoalieStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS core.mv_goalie_game_stats;");
	        
            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW IF NOT EXISTS core.mv_goalie_game_stats
									 AS
									SELECT
										gl.player_id,
										gl.goals_against,
										gl.shots_against,
										gl.saves,
										CASE
											WHEN gl.shutout THEN 1
											ELSE 0
										END AS shutouts,
										CASE
											WHEN gl.win THEN 1
											ELSE 0
										END AS wins,
										CASE
											WHEN shots_against = 0 THEN 0
											ELSE saves / shots_against::FLOAT
										END AS save_percentage,
										gl.goals_against::FLOAT AS goals_against_average,
										gl.goals,
										gl.assists,
										gl.goals + gl.assists AS points,
										gl.penalty_minutes,
										a.id AS account_id,
										a.first_name,
										a.last_name,
										p.""position"",
										p.jersey_number,
										a.birthday,
										a.profile_picture,
										gl.team_id,
										team.name AS team_name,
										team.name_short AS team_name_short,
										team.abbreviation AS team_abbreviation,
										gl.opponent_team_id AS opponent_id,
										opp.name AS opponent_name,
										opp.name_short AS opponent_name_short,
										opp.abbreviation AS opponent_abbreviation,
										gl.game_id,
										g.game_time,
										g.game_type,
										g.venue AS game_venue,
										g.rink AS game_rink,
										g.tournament_id,
										tr.name AS tournament_name,
										tr.is_active AS tournament_active
									FROM
										core.goalie_game_logs gl
										JOIN core.players p ON gl.player_id = p.id
										JOIN core.accounts a ON p.account_id = a.id
										JOIN core.games g ON gl.game_id = g.id
										JOIN core.teams team ON gl.team_id = team.id
										JOIN core.teams opp ON gl.opponent_team_id = opp.id
										JOIN core.tournaments tr ON g.tournament_id = tr.id
									 WITH DATA;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS core.mv_goalie_game_stats;");
	        
	        migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW IF NOT EXISTS core.mv_goalie_game_stats
									 AS
									SELECT gl.player_id,
									    gl.goals_against,
									    gl.shots_against,
									    gl.saves,
									    gl.shutout,
									    gl.win,
									    gl.goals,
									    gl.assists,
									    gl.goals + gl.assists AS points,
									    gl.penalty_minutes,
									    a.id AS account_id,
									    a.first_name,
									    a.last_name,
									    p.""position"",
									    p.jersey_number,
									    a.birthday,
									    a.profile_picture,
									    gl.team_id,
									    team.name AS team_name,
									    team.name_short AS team_name_short,
									    team.abbreviation AS team_abbreviation,
									    gl.opponent_team_id AS opponent_id,
									    opp.name AS opponent_name,
									    opp.name_short AS opponent_name_short,
									    opp.abbreviation AS opponent_abbreviation,
									    gl.game_id,
									    g.game_time,
									    g.game_type,
									    g.venue AS game_venue,
									    g.rink AS game_rink,
									    g.tournament_id,
									    tr.name AS tournament_name,
									    tr.is_active AS tournament_active
									   FROM core.goalie_game_logs gl
									     JOIN core.players p ON gl.player_id = p.id
									     JOIN core.accounts a ON p.account_id = a.id
									     JOIN core.games g ON gl.game_id = g.id
									     JOIN core.teams team ON gl.team_id = team.id
									     JOIN core.teams opp ON gl.opponent_team_id = opp.id
									     JOIN core.tournaments tr ON g.tournament_id = tr.id
									 WITH DATA;");
        }
    }
}
