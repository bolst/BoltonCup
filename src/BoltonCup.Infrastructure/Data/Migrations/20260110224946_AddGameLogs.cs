using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goalie_game_logs",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    opponent_team_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    goals = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    penalty_minutes = table.Column<double>(type: "double precision", nullable: false),
                    goals_against = table.Column<int>(type: "integer", nullable: false),
                    shots_against = table.Column<int>(type: "integer", nullable: false),
                    saves = table.Column<int>(type: "integer", nullable: false),
                    shutout = table.Column<bool>(type: "boolean", nullable: false),
                    win = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goalie_game_logs", x => x.id);
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
                        name: "FK_goalie_game_logs_teams_opponent_team_id",
                        column: x => x.opponent_team_id,
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
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    opponent_team_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    goals = table.Column<int>(type: "integer", nullable: false),
                    assists = table.Column<int>(type: "integer", nullable: false),
                    penalty_minutes = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skater_game_logs", x => x.id);
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
                        name: "FK_skater_game_logs_teams_opponent_team_id",
                        column: x => x.opponent_team_id,
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
                name: "IX_goalie_game_logs_game_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_goalie_game_logs_opponent_team_id",
                schema: "core",
                table: "goalie_game_logs",
                column: "opponent_team_id");

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
                name: "IX_skater_game_logs_game_id",
                schema: "core",
                table: "skater_game_logs",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_skater_game_logs_opponent_team_id",
                schema: "core",
                table: "skater_game_logs",
                column: "opponent_team_id");

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
