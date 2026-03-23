using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueTournamentRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tournament_registrations_account_id",
                schema: "core",
                table: "tournament_registrations");

            migrationBuilder.CreateIndex(
                name: "IX_tournament_registrations_account_id_tournament_id",
                schema: "core",
                table: "tournament_registrations",
                columns: new[] { "account_id", "tournament_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tournament_registrations_account_id_tournament_id",
                schema: "core",
                table: "tournament_registrations");

            migrationBuilder.CreateIndex(
                name: "IX_tournament_registrations_account_id",
                schema: "core",
                table: "tournament_registrations",
                column: "account_id");
        }
    }
}
