using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IsCompleteTournamentRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_complete",
                schema: "core",
                table: "tournament_registrations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_complete",
                schema: "core",
                table: "tournament_registrations");
        }
    }
}
