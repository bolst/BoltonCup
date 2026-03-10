using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IForgotMappingsForPrevious : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LogoS3Key",
                schema: "core",
                table: "tournaments",
                newName: "logo_s3_key");

            migrationBuilder.RenameColumn(
                name: "PenaltySongS3Key",
                schema: "core",
                table: "teams",
                newName: "penalty_song_s3_key");

            migrationBuilder.RenameColumn(
                name: "LogoS3Key",
                schema: "core",
                table: "teams",
                newName: "logo_s3_key");

            migrationBuilder.RenameColumn(
                name: "GoalSongS3Key",
                schema: "core",
                table: "teams",
                newName: "goal_song_s3_key");

            migrationBuilder.RenameColumn(
                name: "BannerS3Key",
                schema: "core",
                table: "teams",
                newName: "banner_s3_key");

            migrationBuilder.RenameColumn(
                name: "ProfilePictureS3Key",
                schema: "core",
                table: "accounts",
                newName: "profile_picture_s3_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "logo_s3_key",
                schema: "core",
                table: "tournaments",
                newName: "LogoS3Key");

            migrationBuilder.RenameColumn(
                name: "penalty_song_s3_key",
                schema: "core",
                table: "teams",
                newName: "PenaltySongS3Key");

            migrationBuilder.RenameColumn(
                name: "logo_s3_key",
                schema: "core",
                table: "teams",
                newName: "LogoS3Key");

            migrationBuilder.RenameColumn(
                name: "goal_song_s3_key",
                schema: "core",
                table: "teams",
                newName: "GoalSongS3Key");

            migrationBuilder.RenameColumn(
                name: "banner_s3_key",
                schema: "core",
                table: "teams",
                newName: "BannerS3Key");

            migrationBuilder.RenameColumn(
                name: "profile_picture_s3_key",
                schema: "core",
                table: "accounts",
                newName: "ProfilePictureS3Key");
        }
    }
}
