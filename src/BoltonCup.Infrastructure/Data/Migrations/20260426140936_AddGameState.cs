using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "game_type",
                schema: "core",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "Round robin",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "game_state",
                schema: "core",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "game_state",
                schema: "core",
                table: "games");

            migrationBuilder.AlterColumn<string>(
                name: "game_type",
                schema: "core",
                table: "games",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Round robin");
        }
    }
}
