using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class EventTeamRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "team_id",
                schema: "core",
                table: "penalties",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "team_id",
                schema: "core",
                table: "goals",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_penalties_team_id",
                schema: "core",
                table: "penalties",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_team_id",
                schema: "core",
                table: "goals",
                column: "team_id");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_teams_team_id",
                schema: "core",
                table: "goals",
                column: "team_id",
                principalSchema: "core",
                principalTable: "teams",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_penalties_teams_team_id",
                schema: "core",
                table: "penalties",
                column: "team_id",
                principalSchema: "core",
                principalTable: "teams",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_teams_team_id",
                schema: "core",
                table: "goals");

            migrationBuilder.DropForeignKey(
                name: "FK_penalties_teams_team_id",
                schema: "core",
                table: "penalties");

            migrationBuilder.DropIndex(
                name: "IX_penalties_team_id",
                schema: "core",
                table: "penalties");

            migrationBuilder.DropIndex(
                name: "IX_goals_team_id",
                schema: "core",
                table: "goals");

            migrationBuilder.DropColumn(
                name: "team_id",
                schema: "core",
                table: "penalties");

            migrationBuilder.DropColumn(
                name: "team_id",
                schema: "core",
                table: "goals");
        }
    }
}
