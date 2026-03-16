using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatisticsPlayerAccountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE INDEX idx_mv_skater_game_stats__player ON core.mv_skater_game_stats(player_id);");
            migrationBuilder.Sql("CREATE INDEX idx_mv_goalie_game_stats__player ON core.mv_goalie_game_stats(player_id);");
            migrationBuilder.Sql("CREATE INDEX idx_mv_skater_game_stats__account ON core.mv_skater_game_stats(account_id);");
            migrationBuilder.Sql("CREATE INDEX idx_mv_goalie_game_stats__account ON core.mv_goalie_game_stats(account_id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_skater_game_stats__player");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_goalie_game_stats__player");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_skater_game_stats__account");
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.idx_mv_goalie_game_stats__account");
        }
    }
}
