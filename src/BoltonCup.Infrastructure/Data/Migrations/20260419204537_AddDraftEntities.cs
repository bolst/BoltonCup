using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drafts",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    draft_type = table.Column<string>(type: "text", nullable: false),
                    draft_status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drafts", x => x.id);
                    table.ForeignKey(
                        name: "FK_drafts_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "draft_orders",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    draft_id = table.Column<int>(type: "integer", nullable: false),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    pick_number = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_draft_orders_drafts_draft_id",
                        column: x => x.draft_id,
                        principalSchema: "core",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_draft_orders_teams_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "draft_picks",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    draft_id = table.Column<int>(type: "integer", nullable: false),
                    overall_pick_number = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_picks", x => x.id);
                    table.ForeignKey(
                        name: "FK_draft_picks_drafts_draft_id",
                        column: x => x.draft_id,
                        principalSchema: "core",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_draft_picks_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_draft_picks_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_draft_orders_draft_id_pick_number",
                schema: "core",
                table: "draft_orders",
                columns: new[] { "draft_id", "pick_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draft_orders_draft_id_tournament_id",
                schema: "core",
                table: "draft_orders",
                columns: new[] { "draft_id", "tournament_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draft_orders_tournament_id",
                schema: "core",
                table: "draft_orders",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_draft_picks_draft_id_overall_pick_number",
                schema: "core",
                table: "draft_picks",
                columns: new[] { "draft_id", "overall_pick_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draft_picks_draft_id_player_id",
                schema: "core",
                table: "draft_picks",
                columns: new[] { "draft_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draft_picks_player_id",
                schema: "core",
                table: "draft_picks",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_draft_picks_team_id",
                schema: "core",
                table: "draft_picks",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_tournament_id",
                schema: "core",
                table: "drafts",
                column: "tournament_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_orders",
                schema: "core");

            migrationBuilder.DropTable(
                name: "draft_picks",
                schema: "core");

            migrationBuilder.DropTable(
                name: "drafts",
                schema: "core");
        }
    }
}
