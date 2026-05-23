using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDraftOrderTeamIdMapTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_draft_orders_teams_tournament_id",
                schema: "core",
                table: "draft_orders");

            migrationBuilder.RenameColumn(
                name: "tournament_id",
                schema: "core",
                table: "draft_orders",
                newName: "team_id");

            migrationBuilder.RenameIndex(
                name: "IX_draft_orders_tournament_id",
                schema: "core",
                table: "draft_orders",
                newName: "IX_draft_orders_team_id");

            migrationBuilder.RenameIndex(
                name: "IX_draft_orders_draft_id_tournament_id",
                schema: "core",
                table: "draft_orders",
                newName: "IX_draft_orders_draft_id_team_id");

            migrationBuilder.AddForeignKey(
                name: "FK_draft_orders_teams_team_id",
                schema: "core",
                table: "draft_orders",
                column: "team_id",
                principalSchema: "core",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_draft_orders_teams_team_id",
                schema: "core",
                table: "draft_orders");

            migrationBuilder.RenameColumn(
                name: "team_id",
                schema: "core",
                table: "draft_orders",
                newName: "tournament_id");

            migrationBuilder.RenameIndex(
                name: "IX_draft_orders_team_id",
                schema: "core",
                table: "draft_orders",
                newName: "IX_draft_orders_tournament_id");

            migrationBuilder.RenameIndex(
                name: "IX_draft_orders_draft_id_team_id",
                schema: "core",
                table: "draft_orders",
                newName: "IX_draft_orders_draft_id_tournament_id");

            migrationBuilder.AddForeignKey(
                name: "FK_draft_orders_teams_tournament_id",
                schema: "core",
                table: "draft_orders",
                column: "tournament_id",
                principalSchema: "core",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
