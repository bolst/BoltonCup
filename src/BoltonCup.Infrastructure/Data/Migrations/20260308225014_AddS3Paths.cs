using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddS3Paths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoS3Key",
                schema: "core",
                table: "tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerS3Key",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoalSongS3Key",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoS3Key",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PenaltySongS3Key",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureS3Key",
                schema: "core",
                table: "accounts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoS3Key",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropColumn(
                name: "BannerS3Key",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "GoalSongS3Key",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "LogoS3Key",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "PenaltySongS3Key",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "ProfilePictureS3Key",
                schema: "core",
                table: "accounts");
        }
    }
}
