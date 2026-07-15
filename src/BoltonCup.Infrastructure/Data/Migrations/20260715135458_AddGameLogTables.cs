using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameLogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goalie_game_logs",
                schema: "core",
                columns: table => new
                {
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    goals_against = table.Column<int>(type: "integer", nullable: false),
                    shots_against = table.Column<int>(type: "integer", nullable: false),
                    saves = table.Column<int>(type: "integer", nullable: false),
                    shutouts = table.Column<int>(type: "integer", nullable: false),
                    wins = table.Column<int>(type: "integer", nullable: false),
                    save_percentage = table.Column<double>(type: "double precision", nullable: false),
                    goals_against_average = table.Column<double>(type: "double precision", nullable: false),
                    games_played = table.Column<int>(type: "integer", nullable: false),
                    goals = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    penalty_minutes = table.Column<double>(type: "double precision", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    position = table.Column<string>(type: "text", nullable: true),
                    jersey_number = table.Column<int>(type: "integer", nullable: true),
                    birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    profile_picture = table.Column<string>(type: "text", nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    team_name = table.Column<string>(type: "text", nullable: true),
                    team_name_short = table.Column<string>(type: "text", nullable: true),
                    team_abbreviation = table.Column<string>(type: "text", nullable: true),
                    team_logo_url = table.Column<string>(type: "text", nullable: true),
                    opponent_id = table.Column<int>(type: "integer", nullable: false),
                    opponent_name = table.Column<string>(type: "text", nullable: true),
                    opponent_name_short = table.Column<string>(type: "text", nullable: true),
                    opponent_abbreviation = table.Column<string>(type: "text", nullable: true),
                    opponent_logo_url = table.Column<string>(type: "text", nullable: true),
                    game_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    game_type = table.Column<string>(type: "text", nullable: true),
                    game_venue = table.Column<string>(type: "text", nullable: true),
                    game_rink = table.Column<string>(type: "text", nullable: true),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    tournament_name = table.Column<string>(type: "text", nullable: true),
                    tournament_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goalie_game_logs", x => new { x.game_id, x.player_id });
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_teams_opponent_id",
                        column: x => x.opponent_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goalie_game_logs_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skater_game_logs",
                schema: "core",
                columns: table => new
                {
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    games_played = table.Column<int>(type: "integer", nullable: false),
                    goals = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    penalty_minutes = table.Column<double>(type: "double precision", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    position = table.Column<string>(type: "text", nullable: true),
                    jersey_number = table.Column<int>(type: "integer", nullable: true),
                    birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    profile_picture = table.Column<string>(type: "text", nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    team_name = table.Column<string>(type: "text", nullable: true),
                    team_name_short = table.Column<string>(type: "text", nullable: true),
                    team_abbreviation = table.Column<string>(type: "text", nullable: true),
                    team_logo_url = table.Column<string>(type: "text", nullable: true),
                    opponent_id = table.Column<int>(type: "integer", nullable: false),
                    opponent_name = table.Column<string>(type: "text", nullable: true),
                    opponent_name_short = table.Column<string>(type: "text", nullable: true),
                    opponent_abbreviation = table.Column<string>(type: "text", nullable: true),
                    opponent_logo_url = table.Column<string>(type: "text", nullable: true),
                    game_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    game_type = table.Column<string>(type: "text", nullable: true),
                    game_venue = table.Column<string>(type: "text", nullable: true),
                    game_rink = table.Column<string>(type: "text", nullable: true),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    tournament_name = table.Column<string>(type: "text", nullable: true),
                    tournament_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skater_game_logs", x => new { x.game_id, x.player_id });
                    table.ForeignKey(
                        name: "FK_skater_game_logs_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_teams_opponent_id",
                        column: x => x.opponent_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skater_game_logs_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_opponent_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "opponent_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_player_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_team_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_opponent_id",
                schema: "core",
                table: "skater_game_logs",
                column: "opponent_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_player_id",
                schema: "core",
                table: "skater_game_logs",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_team_id",
                schema: "core",
                table: "skater_game_logs",
                column: "team_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goalie_game_logs",
                schema: "core");

            migrationBuilder.DropTable(
                name: "skater_game_logs",
                schema: "core");
        }
    }
}
