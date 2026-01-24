using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequirePlayerAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_accounts_account_id",
                schema: "core",
                table: "players");

            migrationBuilder.AlterColumn<int>(
                name: "account_id",
                schema: "core",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_players_accounts_account_id",
                schema: "core",
                table: "players",
                column: "account_id",
                principalSchema: "core",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_accounts_account_id",
                schema: "core",
                table: "players");

            migrationBuilder.AlterColumn<int>(
                name: "account_id",
                schema: "core",
                table: "players",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_players_accounts_account_id",
                schema: "core",
                table: "players",
                column: "account_id",
                principalSchema: "core",
                principalTable: "accounts",
                principalColumn: "id");
        }
    }
}
