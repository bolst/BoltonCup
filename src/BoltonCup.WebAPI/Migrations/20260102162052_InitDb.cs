using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "account",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    profile_picture = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tournaments",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    winning_team_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_registration_open = table.Column<bool>(type: "boolean", nullable: false),
                    is_payment_open = table.Column<bool>(type: "boolean", nullable: false),
                    skater_payment_link = table.Column<string>(type: "text", nullable: true),
                    goalie_payment_link = table.Column<string>(type: "text", nullable: true),
                    skater_limit = table.Column<int>(type: "integer", nullable: true),
                    goalie_limit = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournaments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_short = table.Column<string>(type: "text", nullable: false),
                    abbreviation = table.Column<string>(type: "text", nullable: false),
                    tournament_id = table.Column<int>(type: "integer", nullable: true),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    banner_url = table.Column<string>(type: "text", nullable: true),
                    primary_hex = table.Column<string>(type: "text", nullable: false),
                    secondary_hex = table.Column<string>(type: "text", nullable: false),
                    tertiary_hex = table.Column<string>(type: "text", nullable: true),
                    goal_song_url = table.Column<string>(type: "text", nullable: true),
                    penalty_song_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "games",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    game_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    home_team_id = table.Column<int>(type: "integer", nullable: true),
                    away_team_id = table.Column<int>(type: "integer", nullable: true),
                    game_type = table.Column<string>(type: "text", nullable: true),
                    venue = table.Column<string>(type: "text", nullable: true),
                    rink = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                    table.ForeignKey(
                        name: "FK_games_teams_away_team_id",
                        column: x => x.away_team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_games_teams_home_team_id",
                        column: x => x.home_team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_games_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "players",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    position = table.Column<string>(type: "text", nullable: true),
                    preferred_beer = table.Column<string>(type: "text", nullable: true),
                    jersey_number = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                    table.ForeignKey(
                        name: "FK_players_account_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_players_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_players_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    period_number = table.Column<int>(type: "integer", nullable: false),
                    period_label = table.Column<string>(type: "text", nullable: false),
                    period_time_remaining = table.Column<TimeSpan>(type: "interval", nullable: false),
                    goal_player_id = table.Column<int>(type: "integer", nullable: false),
                    assist1_player_id = table.Column<int>(type: "integer", nullable: true),
                    assist2_player_id = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_goals_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goals_players_assist1_player_id",
                        column: x => x.assist1_player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_goals_players_assist2_player_id",
                        column: x => x.assist2_player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_goals_players_goal_player_id",
                        column: x => x.goal_player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "penalties",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    period_number = table.Column<int>(type: "integer", nullable: false),
                    period_label = table.Column<string>(type: "text", nullable: false),
                    period_time_remaining = table.Column<TimeSpan>(type: "interval", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    infraction_name = table.Column<string>(type: "text", nullable: false),
                    duration_mins = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_penalties", x => x.id);
                    table.ForeignKey(
                        name: "FK_penalties_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_penalties_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_games_away_team_id",
                schema: "core",
                table: "games",
                column: "away_team_id");

            migrationBuilder.CreateIndex(
                name: "IX_games_home_team_id",
                schema: "core",
                table: "games",
                column: "home_team_id");

            migrationBuilder.CreateIndex(
                name: "IX_games_tournament_id",
                schema: "core",
                table: "games",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_assist1_player_id",
                schema: "core",
                table: "goals",
                column: "assist1_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_assist2_player_id",
                schema: "core",
                table: "goals",
                column: "assist2_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_game_id",
                schema: "core",
                table: "goals",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_goals_goal_player_id",
                schema: "core",
                table: "goals",
                column: "goal_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_penalties_game_id",
                schema: "core",
                table: "penalties",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_penalties_player_id",
                schema: "core",
                table: "penalties",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_account_id",
                schema: "core",
                table: "players",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_team_id",
                schema: "core",
                table: "players",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_tournament_id",
                schema: "core",
                table: "players",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_tournament_id",
                schema: "core",
                table: "teams",
                column: "tournament_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goals",
                schema: "core");

            migrationBuilder.DropTable(
                name: "penalties",
                schema: "core");

            migrationBuilder.DropTable(
                name: "games",
                schema: "core");

            migrationBuilder.DropTable(
                name: "players",
                schema: "core");

            migrationBuilder.DropTable(
                name: "account",
                schema: "core");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tournaments",
                schema: "core");
        }
    }
}
