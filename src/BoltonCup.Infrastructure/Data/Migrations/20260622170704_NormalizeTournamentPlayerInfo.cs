using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTournamentPlayerInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payload",
                schema: "core",
                table: "tournament_player_info");

            migrationBuilder.AddColumn<string>(
                name: "song_album_art_url",
                schema: "core",
                table: "tournament_player_info",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "song_artist",
                schema: "core",
                table: "tournament_player_info",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "song_name",
                schema: "core",
                table: "tournament_player_info",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "song_track_id",
                schema: "core",
                table: "tournament_player_info",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tournament_player_game_availability",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tournament_player_info_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    availability = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_player_game_availability", x => x.id);
                    table.ForeignKey(
                        name: "FK_tournament_player_game_availability_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tournament_player_game_availability_tournament_player_info_~",
                        column: x => x.tournament_player_info_id,
                        principalSchema: "core",
                        principalTable: "tournament_player_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tournament_player_game_availability_game_id",
                schema: "core",
                table: "tournament_player_game_availability",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_tournament_player_game_availability_tournament_player_info_~",
                schema: "core",
                table: "tournament_player_game_availability",
                columns: new[] { "tournament_player_info_id", "game_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tournament_player_game_availability",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "song_album_art_url",
                schema: "core",
                table: "tournament_player_info");

            migrationBuilder.DropColumn(
                name: "song_artist",
                schema: "core",
                table: "tournament_player_info");

            migrationBuilder.DropColumn(
                name: "song_name",
                schema: "core",
                table: "tournament_player_info");

            migrationBuilder.DropColumn(
                name: "song_track_id",
                schema: "core",
                table: "tournament_player_info");

            migrationBuilder.AddColumn<string>(
                name: "payload",
                schema: "core",
                table: "tournament_player_info",
                type: "jsonb",
                nullable: true);
        }
    }
}
