using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GeneralizeHighlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename in place rather than drop/create so existing highlight rows survive.
            migrationBuilder.RenameTable(
                name: "game_highlights",
                schema: "core",
                newName: "highlights",
                newSchema: "core");

            migrationBuilder.RenameIndex(
                schema: "core",
                table: "highlights",
                name: "IX_game_highlights_video_id",
                newName: "IX_highlights_video_id");

            migrationBuilder.Sql(
                "ALTER TABLE core.highlights RENAME CONSTRAINT \"PK_game_highlights\" TO \"PK_highlights\";");

            migrationBuilder.CreateTable(
                name: "tag_labels",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag_labels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "highlight_tags",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    highlight_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: true),
                    player_id = table.Column<int>(type: "integer", nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    tournament_id = table.Column<int>(type: "integer", nullable: true),
                    label_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_highlight_tags", x => x.id);
                    table.CheckConstraint("CK_highlight_tags_exactly_one_target", "num_nonnulls(game_id, player_id, team_id, tournament_id, label_id) = 1");
                    table.ForeignKey(
                        name: "FK_highlight_tags_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_highlight_tags_highlights_highlight_id",
                        column: x => x.highlight_id,
                        principalSchema: "core",
                        principalTable: "highlights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_highlight_tags_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_highlight_tags_tag_labels_label_id",
                        column: x => x.label_id,
                        principalSchema: "core",
                        principalTable: "tag_labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_highlight_tags_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_highlight_tags_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_game_id",
                schema: "core",
                table: "highlight_tags",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id",
                schema: "core",
                table: "highlight_tags",
                column: "highlight_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_label_id",
                schema: "core",
                table: "highlight_tags",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_player_id",
                schema: "core",
                table: "highlight_tags",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_team_id",
                schema: "core",
                table: "highlight_tags",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_tournament_id",
                schema: "core",
                table: "highlight_tags",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_labels_name",
                schema: "core",
                table: "tag_labels",
                column: "name",
                unique: true);

            // Convert the old single game/player columns into tag rows before dropping them.
            migrationBuilder.Sql(
                """
                INSERT INTO core.highlight_tags (highlight_id, game_id, created_at, created_by)
                SELECT id, game_id, created_at, created_by FROM core.highlights;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO core.highlight_tags (highlight_id, player_id, created_at, created_by)
                SELECT id, player_id, created_at, created_by FROM core.highlights WHERE player_id IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_game_highlights_games_game_id",
                schema: "core",
                table: "highlights");

            migrationBuilder.DropForeignKey(
                name: "FK_game_highlights_players_player_id",
                schema: "core",
                table: "highlights");

            migrationBuilder.DropIndex(
                name: "IX_game_highlights_game_id",
                schema: "core",
                table: "highlights");

            migrationBuilder.DropIndex(
                name: "IX_game_highlights_player_id",
                schema: "core",
                table: "highlights");

            migrationBuilder.DropColumn(
                name: "game_id",
                schema: "core",
                table: "highlights");

            migrationBuilder.DropColumn(
                name: "player_id",
                schema: "core",
                table: "highlights");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "game_id",
                schema: "core",
                table: "highlights",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "player_id",
                schema: "core",
                table: "highlights",
                type: "integer",
                nullable: true);

            // Lossy, and deliberately non-destructive about it:
            //   - a highlight tagged with several games keeps only its most recent one;
            //   - a highlight tagged with several players keeps the lowest tag id;
            //   - team, tournament and label tags are discarded (tag_labels is dropped);
            //   - highlights with no game tag keep a NULL game_id rather than being deleted.
            // The old schema had game_id NOT NULL, so this rollback intentionally leaves the
            // column nullable: deleting real highlights to satisfy a constraint is worse than
            // relaxing it.
            migrationBuilder.Sql(
                """
                UPDATE core.highlights h
                SET game_id = t.game_id
                FROM (
                    SELECT DISTINCT ON (ht.highlight_id) ht.highlight_id, ht.game_id
                    FROM core.highlight_tags ht
                    JOIN core.games g ON g.id = ht.game_id
                    WHERE ht.game_id IS NOT NULL
                    ORDER BY ht.highlight_id, g.game_time DESC
                ) t
                WHERE h.id = t.highlight_id;
                """);

            migrationBuilder.Sql(
                """
                UPDATE core.highlights h
                SET player_id = t.player_id
                FROM (
                    SELECT DISTINCT ON (highlight_id) highlight_id, player_id
                    FROM core.highlight_tags
                    WHERE player_id IS NOT NULL
                    ORDER BY highlight_id, id
                ) t
                WHERE h.id = t.highlight_id;
                """);

            migrationBuilder.DropTable(
                name: "highlight_tags",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tag_labels",
                schema: "core");

            migrationBuilder.CreateIndex(
                name: "IX_game_highlights_game_id",
                schema: "core",
                table: "highlights",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_highlights_player_id",
                schema: "core",
                table: "highlights",
                column: "player_id");

            migrationBuilder.AddForeignKey(
                name: "FK_game_highlights_games_game_id",
                schema: "core",
                table: "highlights",
                column: "game_id",
                principalSchema: "core",
                principalTable: "games",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_game_highlights_players_player_id",
                schema: "core",
                table: "highlights",
                column: "player_id",
                principalSchema: "core",
                principalTable: "players",
                principalColumn: "id");

            migrationBuilder.Sql(
                "ALTER TABLE core.highlights RENAME CONSTRAINT \"PK_highlights\" TO \"PK_game_highlights\";");

            migrationBuilder.RenameIndex(
                schema: "core",
                table: "highlights",
                name: "IX_highlights_video_id",
                newName: "IX_game_highlights_video_id");

            migrationBuilder.RenameTable(
                name: "highlights",
                schema: "core",
                newName: "game_highlights",
                newSchema: "core");
        }
    }
}
