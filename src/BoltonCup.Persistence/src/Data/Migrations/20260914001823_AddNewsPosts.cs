using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news_posts",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true),
                    markdown_content = table.Column<string>(type: "text", nullable: true),
                    cover_image_key = table.Column<string>(type: "text", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "news_post_tags",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    news_post_id = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: true),
                    account_id = table.Column<int>(type: "integer", nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    tournament_id = table.Column<int>(type: "integer", nullable: true),
                    label_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_post_tags", x => x.id);
                    table.CheckConstraint("CK_news_post_tags_exactly_one_target", "num_nonnulls(game_id, account_id, team_id, tournament_id, label_id) = 1");
                    table.ForeignKey(
                        name: "FK_news_post_tags_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_post_tags_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "core",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_post_tags_news_posts_news_post_id",
                        column: x => x.news_post_id,
                        principalSchema: "core",
                        principalTable: "news_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_post_tags_tag_labels_label_id",
                        column: x => x.label_id,
                        principalSchema: "core",
                        principalTable: "tag_labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_post_tags_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_post_tags_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_account_id",
                schema: "core",
                table: "news_post_tags",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_game_id",
                schema: "core",
                table: "news_post_tags",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_label_id",
                schema: "core",
                table: "news_post_tags",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_news_post_id",
                schema: "core",
                table: "news_post_tags",
                column: "news_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_news_post_id_account_id",
                schema: "core",
                table: "news_post_tags",
                columns: new[] { "news_post_id", "account_id" },
                unique: true,
                filter: "account_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_news_post_id_game_id",
                schema: "core",
                table: "news_post_tags",
                columns: new[] { "news_post_id", "game_id" },
                unique: true,
                filter: "game_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_news_post_id_label_id",
                schema: "core",
                table: "news_post_tags",
                columns: new[] { "news_post_id", "label_id" },
                unique: true,
                filter: "label_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_news_post_id_team_id",
                schema: "core",
                table: "news_post_tags",
                columns: new[] { "news_post_id", "team_id" },
                unique: true,
                filter: "team_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_news_post_id_tournament_id",
                schema: "core",
                table: "news_post_tags",
                columns: new[] { "news_post_id", "tournament_id" },
                unique: true,
                filter: "tournament_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_team_id",
                schema: "core",
                table: "news_post_tags",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_news_post_tags_tournament_id",
                schema: "core",
                table: "news_post_tags",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "IX_news_posts_is_published_published_at",
                schema: "core",
                table: "news_posts",
                columns: new[] { "is_published", "published_at" });

            migrationBuilder.CreateIndex(
                name: "IX_news_posts_slug",
                schema: "core",
                table: "news_posts",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_post_tags",
                schema: "core");

            migrationBuilder.DropTable(
                name: "news_posts",
                schema: "core");
        }
    }
}
