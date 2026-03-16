using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReapplyStatisticIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE INDEX idx_mv_skater_game_stats__tournament ON core.mv_skater_game_stats(tournament_id);");
            migrationBuilder.Sql("CREATE INDEX idx_mv_goalie_game_stats__tournament ON core.mv_goalie_game_stats(tournament_id);");
            
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_skater_game_stats__unique ON core.mv_skater_game_stats(player_id, game_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_goalie_game_stats__unique ON core.mv_goalie_game_stats(player_id, game_id);");
            
            migrationBuilder.Sql("CREATE INDEX idx_mv_skater_game_stats__player ON core.mv_skater_game_stats(player_id);");
            migrationBuilder.Sql("CREATE INDEX idx_mv_goalie_game_stats__player ON core.mv_goalie_game_stats(player_id);");
            
            migrationBuilder.Sql("CREATE INDEX idx_mv_skater_game_stats__account ON core.mv_skater_game_stats(account_id);");
            migrationBuilder.Sql("CREATE INDEX idx_mv_goalie_game_stats__account ON core.mv_goalie_game_stats(account_id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_skater_game_stats__tournament;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_goalie_game_stats__tournament;");
            
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_skater_game_stats__unique;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_goalie_game_stats__unique;");
            
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_skater_game_stats__player;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_goalie_game_stats__player;");
            
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_skater_game_stats__account;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_goalie_game_stats__account;");
        }
    }
}
