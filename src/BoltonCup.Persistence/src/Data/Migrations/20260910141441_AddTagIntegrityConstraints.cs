using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTagIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tag_labels_name",
                schema: "core",
                table: "tag_labels");

            migrationBuilder.CreateIndex(
                name: "IX_tag_labels_name",
                schema: "core",
                table: "tag_labels",
                column: "name");

            // Case-insensitive uniqueness, matching TagLabelService's lookup. An expression index
            // is not expressible through the fluent API, so it is created directly here.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_tag_labels_name_lower\" ON core.tag_labels (lower(name));");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_game_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "game_id" },
                unique: true,
                filter: "game_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_label_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "label_id" },
                unique: true,
                filter: "label_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_player_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "player_id" },
                unique: true,
                filter: "player_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_team_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "team_id" },
                unique: true,
                filter: "team_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_tournament_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "tournament_id" },
                unique: true,
                filter: "tournament_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS core.\"IX_tag_labels_name_lower\";");

            migrationBuilder.DropIndex(
                name: "IX_tag_labels_name",
                schema: "core",
                table: "tag_labels");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_game_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_label_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_player_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_team_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_tournament_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.CreateIndex(
                name: "IX_tag_labels_name",
                schema: "core",
                table: "tag_labels",
                column: "name",
                unique: true);
        }
    }
}
