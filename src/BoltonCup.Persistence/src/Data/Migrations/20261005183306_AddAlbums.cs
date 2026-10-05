using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "album_image_tags",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    album_image_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: true),
                    account_id = table.Column<int>(type: "integer", nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    tournament_id = table.Column<int>(type: "integer", nullable: true),
                    label_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_album_image_tags", x => x.id);
                    table.CheckConstraint("CK_album_image_tags_exactly_one_target", "num_nonnulls(game_id, account_id, team_id, tournament_id, label_id) = 1");
                    table.ForeignKey(
                        name: "FK_album_image_tags_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_image_tags_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_image_tags_tag_labels_label_id",
                        column: x => x.label_id,
                        principalSchema: "core",
                        principalTable: "tag_labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_image_tags_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_image_tags_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "album_images",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    album_id = table.Column<int>(type: "integer", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_album_images", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "albums",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cover_image_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_albums", x => x.id);
                    table.CheckConstraint("CK_albums_published_has_date", "NOT is_published OR published_at IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_albums_album_images_cover_image_id",
                        column: x => x.cover_image_id,
                        principalSchema: "core",
                        principalTable: "album_images",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "album_tags",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    album_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: true),
                    account_id = table.Column<int>(type: "integer", nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    tournament_id = table.Column<int>(type: "integer", nullable: true),
                    label_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_album_tags", x => x.id);
                    table.CheckConstraint("CK_album_tags_exactly_one_target", "num_nonnulls(game_id, account_id, team_id, tournament_id, label_id) = 1");
                    table.ForeignKey(
                        name: "FK_album_tags_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_tags_albums_album_id",
                        column: x => x.album_id,
                        principalSchema: "core",
                        principalTable: "albums",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_tags_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_tags_tag_labels_label_id",
                        column: x => x.label_id,
                        principalSchema: "core",
                        principalTable: "tag_labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_tags_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_album_tags_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_account_id",
                schema: "core",
                table: "album_image_tags",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_album_image_id",
                schema: "core",
                table: "album_image_tags",
                column: "album_image_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_album_image_id_account_id",
                schema: "core",
                table: "album_image_tags",
                columns: new[] { "album_image_id", "account_id" },
                unique: true,
                filter: "account_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_album_image_id_game_id",
                schema: "core",
                table: "album_image_tags",
                columns: new[] { "album_image_id", "game_id" },
                unique: true,
                filter: "game_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_album_image_id_label_id",
                schema: "core",
                table: "album_image_tags",
                columns: new[] { "album_image_id", "label_id" },
                unique: true,
                filter: "label_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_album_image_id_team_id",
                schema: "core",
                table: "album_image_tags",
                columns: new[] { "album_image_id", "team_id" },
                unique: true,
                filter: "team_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_album_image_id_tournament_id",
                schema: "core",
                table: "album_image_tags",
                columns: new[] { "album_image_id", "tournament_id" },
                unique: true,
                filter: "tournament_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_game_id",
                schema: "core",
                table: "album_image_tags",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_label_id",
                schema: "core",
                table: "album_image_tags",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_team_id",
                schema: "core",
                table: "album_image_tags",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_image_tags_tournament_id",
                schema: "core",
                table: "album_image_tags",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_images_album_id_sort_order",
                schema: "core",
                table: "album_images",
                columns: new[] { "album_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_account_id",
                schema: "core",
                table: "album_tags",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_album_id",
                schema: "core",
                table: "album_tags",
                column: "album_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_album_id_account_id",
                schema: "core",
                table: "album_tags",
                columns: new[] { "album_id", "account_id" },
                unique: true,
                filter: "account_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_album_id_game_id",
                schema: "core",
                table: "album_tags",
                columns: new[] { "album_id", "game_id" },
                unique: true,
                filter: "game_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_album_id_label_id",
                schema: "core",
                table: "album_tags",
                columns: new[] { "album_id", "label_id" },
                unique: true,
                filter: "label_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_album_id_team_id",
                schema: "core",
                table: "album_tags",
                columns: new[] { "album_id", "team_id" },
                unique: true,
                filter: "team_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_album_id_tournament_id",
                schema: "core",
                table: "album_tags",
                columns: new[] { "album_id", "tournament_id" },
                unique: true,
                filter: "tournament_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_game_id",
                schema: "core",
                table: "album_tags",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_label_id",
                schema: "core",
                table: "album_tags",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_team_id",
                schema: "core",
                table: "album_tags",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_album_tags_tournament_id",
                schema: "core",
                table: "album_tags",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_albums_cover_image_id",
                schema: "core",
                table: "albums",
                column: "cover_image_id");

            migrationBuilder.CreateIndex(
                name: "IX_albums_is_published_occurred_at",
                schema: "core",
                table: "albums",
                columns: new[] { "is_published", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_albums_slug",
                schema: "core",
                table: "albums",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_album_image_tags_album_images_album_image_id",
                schema: "core",
                table: "album_image_tags",
                column: "album_image_id",
                principalSchema: "core",
                principalTable: "album_images",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_album_images_albums_album_id",
                schema: "core",
                table: "album_images",
                column: "album_id",
                principalSchema: "core",
                principalTable: "albums",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_albums_album_images_cover_image_id",
                schema: "core",
                table: "albums");

            migrationBuilder.DropTable(
                name: "album_image_tags",
                schema: "core");

            migrationBuilder.DropTable(
                name: "album_tags",
                schema: "core");

            migrationBuilder.DropTable(
                name: "album_images",
                schema: "core");

            migrationBuilder.DropTable(
                name: "albums",
                schema: "core");
        }
    }
}
