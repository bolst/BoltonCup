using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NavigatableTournamentChampion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tournaments_winning_team_id",
                schema: "core",
                table: "tournaments",
                column: "winning_team_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tournaments_teams_winning_team_id",
                schema: "core",
                table: "tournaments",
                column: "winning_team_id",
                principalSchema: "core",
                principalTable: "teams",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tournaments_teams_winning_team_id",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropIndex(
                name: "IX_tournaments_winning_team_id",
                schema: "core",
                table: "tournaments");
        }
    }
}
