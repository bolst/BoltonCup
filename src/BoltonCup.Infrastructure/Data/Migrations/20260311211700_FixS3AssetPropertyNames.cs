using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixS3AssetPropertyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "logo_s3_key",
                schema: "core",
                table: "tournaments",
                newName: "logo_key");

            migrationBuilder.RenameColumn(
                name: "penalty_song_s3_key",
                schema: "core",
                table: "teams",
                newName: "penalty_song_key");

            migrationBuilder.RenameColumn(
                name: "logo_s3_key",
                schema: "core",
                table: "teams",
                newName: "logo_key");

            migrationBuilder.RenameColumn(
                name: "goal_song_s3_key",
                schema: "core",
                table: "teams",
                newName: "goal_song_key");

            migrationBuilder.RenameColumn(
                name: "banner_s3_key",
                schema: "core",
                table: "teams",
                newName: "banner_key");

            migrationBuilder.RenameColumn(
                name: "profile_picture_s3_key",
                schema: "core",
                table: "accounts",
                newName: "avatar_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "logo_key",
                schema: "core",
                table: "tournaments",
                newName: "logo_s3_key");

            migrationBuilder.RenameColumn(
                name: "penalty_song_key",
                schema: "core",
                table: "teams",
                newName: "penalty_song_s3_key");

            migrationBuilder.RenameColumn(
                name: "logo_key",
                schema: "core",
                table: "teams",
                newName: "logo_s3_key");

            migrationBuilder.RenameColumn(
                name: "goal_song_key",
                schema: "core",
                table: "teams",
                newName: "goal_song_s3_key");

            migrationBuilder.RenameColumn(
                name: "banner_key",
                schema: "core",
                table: "teams",
                newName: "banner_s3_key");

            migrationBuilder.RenameColumn(
                name: "avatar_key",
                schema: "core",
                table: "accounts",
                newName: "profile_picture_s3_key");
        }
    }
}
