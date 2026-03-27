using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentRegistrationFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "goalie_registration_fee",
                schema: "core",
                table: "tournaments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "skater_registration_fee",
                schema: "core",
                table: "tournaments",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "goalie_registration_fee",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropColumn(
                name: "skater_registration_fee",
                schema: "core",
                table: "tournaments");
        }
    }
}
