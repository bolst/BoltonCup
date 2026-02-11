using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventTeamRequiredRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_teams_team_id",
                schema: "core",
                table: "goals");

            migrationBuilder.DropForeignKey(
                name: "FK_penalties_teams_team_id",
                schema: "core",
                table: "penalties");

            migrationBuilder.AlterColumn<int>(
                name: "team_id",
                schema: "core",
                table: "penalties",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "team_id",
                schema: "core",
                table: "goals",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_goals_teams_team_id",
                schema: "core",
                table: "goals",
                column: "team_id",
                principalSchema: "core",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_penalties_teams_team_id",
                schema: "core",
                table: "penalties",
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
                name: "FK_goals_teams_team_id",
                schema: "core",
                table: "goals");

            migrationBuilder.DropForeignKey(
                name: "FK_penalties_teams_team_id",
                schema: "core",
                table: "penalties");

            migrationBuilder.AlterColumn<int>(
                name: "team_id",
                schema: "core",
                table: "penalties",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "team_id",
                schema: "core",
                table: "goals",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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
    }
}
