using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPenaltySongTrackRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "penalty_song_track_id",
                schema: "core",
                table: "teams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_penalty_song_track_id",
                schema: "core",
                table: "teams",
                column: "penalty_song_track_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_tournament_music_tracks_penalty_song_track_id",
                schema: "core",
                table: "teams",
                column: "penalty_song_track_id",
                principalSchema: "core",
                principalTable: "tournament_music_tracks",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_tournament_music_tracks_penalty_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_penalty_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "penalty_song_track_id",
                schema: "core",
                table: "teams");
        }
    }
}
