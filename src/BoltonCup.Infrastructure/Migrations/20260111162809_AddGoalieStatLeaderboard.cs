using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalieStatLeaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // build mat view
            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW core.goalie_stats_leaderboard AS
									SELECT
										gl.player_id,
										gl.team_id,
										game.tournament_id,
										count(DISTINCT gl.id) AS games_played,
										sum(gl.goals) AS goals,
										sum(gl.assists) AS assists,
										sum(gl.penalty_minutes) AS penalty_minutes,
										sum(gl.goals_against) AS goals_against,
										sum(gl.goals_against)::FLOAT / count(DISTINCT gl.id) AS goals_against_average,
										0 AS shots_against,
										0 AS saves,
										0::FLOAT AS save_percentage,
										sum(gl.shutout::INTEGER) AS shutouts,
										sum(gl.win::INTEGER) AS wins
									FROM
										core.goalie_game_logs gl
										JOIN core.games game ON game.id = gl.game_id
										LEFT JOIN core.players p ON p.id = gl.player_id
									GROUP BY
										gl.player_id,
										gl.team_id,
										game.tournament_id
									ORDER BY
										save_percentage DESC,
										goals_against_average,
										shutouts DESC,
										wins DESC,
										saves DESC");
            
            // index
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX idx_goalie_leaderboard_id ON core.goalie_stats_leaderboard (player_id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS core.goalie_stats_leaderboard");
        }
    }
}