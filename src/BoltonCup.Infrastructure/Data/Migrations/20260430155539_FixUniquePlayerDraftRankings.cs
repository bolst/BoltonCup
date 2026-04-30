using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUniquePlayerDraftRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_draft_rankings_player_id_tournament_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.CreateIndex(
                name: "IX_draft_rankings_player_id_draft_id",
                schema: "core",
                table: "draft_rankings",
                columns: new[] { "player_id", "draft_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_draft_rankings_player_id_draft_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.CreateIndex(
                name: "IX_draft_rankings_player_id_tournament_id",
                schema: "core",
                table: "draft_rankings",
                columns: new[] { "player_id", "tournament_id" },
                unique: true);
        }
    }
}
