using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkDraftRankingWithPick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDrafted",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.AddColumn<int>(
                name: "draft_pick_id",
                schema: "core",
                table: "draft_rankings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_draft_rankings_draft_pick_id",
                schema: "core",
                table: "draft_rankings",
                column: "draft_pick_id");

            migrationBuilder.AddForeignKey(
                name: "FK_draft_rankings_draft_picks_draft_pick_id",
                schema: "core",
                table: "draft_rankings",
                column: "draft_pick_id",
                principalSchema: "core",
                principalTable: "draft_picks",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_draft_rankings_draft_picks_draft_pick_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropIndex(
                name: "IX_draft_rankings_draft_pick_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropColumn(
                name: "draft_pick_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.AddColumn<bool>(
                name: "IsDrafted",
                schema: "core",
                table: "draft_rankings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
