using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifyMusicQueueIntoTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tournament_music_tracks_tournament_id_provider_type_track_id",
                schema: "core",
                table: "tournament_music_tracks");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "is_in_base_pool",
                schema: "core",
                table: "tournament_music_tracks",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "audio_file_key",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "requested_by_name",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: false,
                // Existing rows are already-uploaded library tracks: backfill as downloaded manual uploads.
                defaultValue: "ManualUpload");

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: false,
                defaultValue: "Downloaded");

            migrationBuilder.CreateIndex(
                name: "IX_tournament_music_tracks_tournament_id_provider_type_track_id",
                schema: "core",
                table: "tournament_music_tracks",
                columns: new[] { "tournament_id", "provider_type", "track_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tournament_music_tracks_tournament_id_provider_type_track_id",
                schema: "core",
                table: "tournament_music_tracks");

            migrationBuilder.DropColumn(
                name: "requested_by_name",
                schema: "core",
                table: "tournament_music_tracks");

            migrationBuilder.DropColumn(
                name: "source",
                schema: "core",
                table: "tournament_music_tracks");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "core",
                table: "tournament_music_tracks");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_in_base_pool",
                schema: "core",
                table: "tournament_music_tracks",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "audio_file_key",
                schema: "core",
                table: "tournament_music_tracks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tournament_music_tracks_tournament_id_provider_type_track_id",
                schema: "core",
                table: "tournament_music_tracks",
                columns: new[] { "tournament_id", "provider_type", "track_id" });
        }
    }
}
