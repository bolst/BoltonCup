using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldAssetUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "banner_url",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "goal_song_url",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "logo_url",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "penalty_song_url",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "profile_picture",
                schema: "core",
                table: "accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "banner_url",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "goal_song_url",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "penalty_song_url",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_picture",
                schema: "core",
                table: "accounts",
                type: "text",
                nullable: true);
        }
    }
}
