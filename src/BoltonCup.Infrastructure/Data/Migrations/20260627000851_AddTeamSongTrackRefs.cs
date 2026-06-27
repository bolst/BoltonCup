using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamSongTrackRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "goal_song_track_id",
                schema: "core",
                table: "teams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "win_song_track_id",
                schema: "core",
                table: "teams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_goal_song_track_id",
                schema: "core",
                table: "teams",
                column: "goal_song_track_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_win_song_track_id",
                schema: "core",
                table: "teams",
                column: "win_song_track_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_tournament_music_tracks_goal_song_track_id",
                schema: "core",
                table: "teams",
                column: "goal_song_track_id",
                principalSchema: "core",
                principalTable: "tournament_music_tracks",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_tournament_music_tracks_win_song_track_id",
                schema: "core",
                table: "teams",
                column: "win_song_track_id",
                principalSchema: "core",
                principalTable: "tournament_music_tracks",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Preserve existing uploaded goal/win song keys: turn each into a downloaded library track and
            // point the team's new reference at it. (penalty_song_key is intentionally dropped, not preserved.)
            migrationBuilder.Sql(@"
                DO $$
                DECLARE r RECORD; new_id INT;
                BEGIN
                    FOR r IN SELECT id, tournament_id, goal_song_key FROM core.teams
                             WHERE goal_song_key IS NOT NULL AND tournament_id IS NOT NULL LOOP
                        INSERT INTO core.tournament_music_tracks
                            (tournament_id, audio_file_key, provider_type, status, source, is_in_base_pool, created_at)
                        VALUES (r.tournament_id, r.goal_song_key, 'Spotify', 'Downloaded', 'PlayerRequest', false, now() AT TIME ZONE 'UTC')
                        RETURNING id INTO new_id;
                        UPDATE core.teams SET goal_song_track_id = new_id WHERE id = r.id;
                    END LOOP;
                    FOR r IN SELECT id, tournament_id, win_song FROM core.teams
                             WHERE win_song IS NOT NULL AND tournament_id IS NOT NULL LOOP
                        INSERT INTO core.tournament_music_tracks
                            (tournament_id, audio_file_key, provider_type, status, source, is_in_base_pool, created_at)
                        VALUES (r.tournament_id, r.win_song, 'Spotify', 'Downloaded', 'PlayerRequest', false, now() AT TIME ZONE 'UTC')
                        RETURNING id INTO new_id;
                        UPDATE core.teams SET win_song_track_id = new_id WHERE id = r.id;
                    END LOOP;
                END $$;");

            migrationBuilder.DropColumn(
                name: "goal_song_key",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "penalty_song_key",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "win_song",
                schema: "core",
                table: "teams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_tournament_music_tracks_goal_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_tournament_music_tracks_win_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_goal_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_win_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "goal_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "win_song_track_id",
                schema: "core",
                table: "teams");

            migrationBuilder.AddColumn<string>(
                name: "goal_song_key",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "penalty_song_key",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "win_song",
                schema: "core",
                table: "teams",
                type: "text",
                nullable: true);
        }
    }
}
