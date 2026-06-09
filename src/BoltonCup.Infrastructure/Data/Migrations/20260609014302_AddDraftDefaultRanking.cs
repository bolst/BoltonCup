using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftDefaultRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "default_custom_ranking_id",
                schema: "core",
                table: "drafts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_drafts_default_custom_ranking_id",
                schema: "core",
                table: "drafts",
                column: "default_custom_ranking_id");

            migrationBuilder.AddForeignKey(
                name: "FK_drafts_custom_rankings_default_custom_ranking_id",
                schema: "core",
                table: "drafts",
                column: "default_custom_ranking_id",
                principalSchema: "core",
                principalTable: "custom_rankings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_drafts_custom_rankings_default_custom_ranking_id",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropIndex(
                name: "IX_drafts_default_custom_ranking_id",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropColumn(
                name: "default_custom_ranking_id",
                schema: "core",
                table: "drafts");
        }
    }
}
