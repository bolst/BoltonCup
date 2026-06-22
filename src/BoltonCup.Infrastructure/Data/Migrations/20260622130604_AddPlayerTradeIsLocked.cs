using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerTradeIsLocked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trade_players_player_id",
                schema: "core",
                table: "trade_players");

            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                schema: "core",
                table: "trade_players",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_trade_players_player_id",
                schema: "core",
                table: "trade_players",
                column: "player_id",
                unique: true,
                filter: "is_locked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trade_players_player_id",
                schema: "core",
                table: "trade_players");

            migrationBuilder.DropColumn(
                name: "is_locked",
                schema: "core",
                table: "trade_players");

            migrationBuilder.CreateIndex(
                name: "IX_trade_players_player_id",
                schema: "core",
                table: "trade_players",
                column: "player_id");
        }
    }
}
