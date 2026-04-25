using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPlayerDraftRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_draft_rankings",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "core",
                table: "draft_rankings",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsDrafted",
                schema: "core",
                table: "draft_rankings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "draft_id",
                schema: "core",
                table: "draft_rankings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_draft_rankings",
                schema: "core",
                table: "draft_rankings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_draft_rankings_draft_id",
                schema: "core",
                table: "draft_rankings",
                column: "draft_id");

            migrationBuilder.CreateIndex(
                name: "IX_draft_rankings_player_id_tournament_id",
                schema: "core",
                table: "draft_rankings",
                columns: new[] { "player_id", "tournament_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_draft_rankings_drafts_draft_id",
                schema: "core",
                table: "draft_rankings",
                column: "draft_id",
                principalSchema: "core",
                principalTable: "drafts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_draft_rankings_drafts_draft_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_draft_rankings",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropIndex(
                name: "IX_draft_rankings_draft_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropIndex(
                name: "IX_draft_rankings_player_id_tournament_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropColumn(
                name: "IsDrafted",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.DropColumn(
                name: "draft_id",
                schema: "core",
                table: "draft_rankings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_draft_rankings",
                schema: "core",
                table: "draft_rankings",
                columns: new[] { "player_id", "tournament_id" });
        }
    }
}
