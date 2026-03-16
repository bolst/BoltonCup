using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkaterStatLeaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // build mat view
            migrationBuilder.Sql(@"CREATE MATERIALIZED VIEW core.skater_stats_leaderboard AS
									SELECT
										gl.player_id,
										gl.team_id,
										game.tournament_id,
										count(DISTINCT gl.id) AS games_played,
										sum(gl.goals) AS goals,
										sum(gl.assists) AS assists,
										sum(gl.goals) + sum(assists) AS points,
										sum(gl.penalty_minutes) AS penalty_minutes
									FROM
										core.skater_game_logs gl
										JOIN core.games game ON game.id = gl.game_id
										LEFT JOIN core.players p ON p.id = gl.player_id
									GROUP BY
										gl.player_id,
										gl.team_id,
										game.tournament_id
									ORDER BY
										points DESC,
										goals DESC,
										assists DESC");
            
            // index
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX idx_skater_leaderboard_id ON core.skater_stats_leaderboard (player_id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS core.skater_stats_leaderboard");
        }
    }
}