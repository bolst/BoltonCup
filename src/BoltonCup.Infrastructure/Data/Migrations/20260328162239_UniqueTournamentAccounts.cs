using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueTournamentAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_account_id",
                schema: "core",
                table: "players");

            migrationBuilder.CreateIndex(
                name: "IX_players_account_id_tournament_id",
                schema: "core",
                table: "players",
                columns: new[] { "account_id", "tournament_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_account_id_tournament_id",
                schema: "core",
                table: "players");

            migrationBuilder.CreateIndex(
                name: "IX_players_account_id",
                schema: "core",
                table: "players",
                column: "account_id");
        }
    }
}
