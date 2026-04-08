using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationDataToPlayerModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "agreed_code_of_conduct",
                schema: "core",
                table: "players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "agreed_communication_consent",
                schema: "core",
                table: "players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "agreed_concussion_waiver",
                schema: "core",
                table: "players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_play_either_position",
                schema: "core",
                table: "players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "friends",
                schema: "core",
                table: "players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "jersey_size",
                schema: "core",
                table: "players",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "agreed_code_of_conduct",
                schema: "core",
                table: "players");

            migrationBuilder.DropColumn(
                name: "agreed_communication_consent",
                schema: "core",
                table: "players");

            migrationBuilder.DropColumn(
                name: "agreed_concussion_waiver",
                schema: "core",
                table: "players");

            migrationBuilder.DropColumn(
                name: "can_play_either_position",
                schema: "core",
                table: "players");

            migrationBuilder.DropColumn(
                name: "friends",
                schema: "core",
                table: "players");

            migrationBuilder.DropColumn(
                name: "jersey_size",
                schema: "core",
                table: "players");
        }
    }
}
