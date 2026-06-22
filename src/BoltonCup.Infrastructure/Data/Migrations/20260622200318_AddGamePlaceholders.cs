using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGamePlaceholders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "away_team_placeholder",
                schema: "core",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "home_team_placeholder",
                schema: "core",
                table: "games",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "away_team_placeholder",
                schema: "core",
                table: "games");

            migrationBuilder.DropColumn(
                name: "home_team_placeholder",
                schema: "core",
                table: "games");
        }
    }
}
