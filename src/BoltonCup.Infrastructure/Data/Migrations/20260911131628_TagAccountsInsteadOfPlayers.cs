using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TagAccountsInsteadOfPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_highlight_tags_players_player_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_player_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropCheckConstraint(
                name: "CK_highlight_tags_exactly_one_target",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.RenameColumn(
                name: "player_id",
                schema: "core",
                table: "highlight_tags",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_highlight_tags_player_id",
                schema: "core",
                table: "highlight_tags",
                newName: "IX_highlight_tags_account_id");

            // The renamed column still holds player ids. Translate them to the owning account
            // before anything constrains the column.
            migrationBuilder.Sql(
                """
                UPDATE core.highlight_tags t
                SET account_id = p.account_id
                FROM core.players p
                WHERE t.account_id = p.id;
                """);

            // One person has a player row per tournament, so a highlight tagged with two of them
            // collapses to one account tag. The oldest row wins.
            migrationBuilder.Sql(
                """
                DELETE FROM core.highlight_tags a
                USING core.highlight_tags b
                WHERE a.account_id IS NOT NULL
                  AND a.highlight_id = b.highlight_id
                  AND a.account_id = b.account_id
                  AND a.id > b.id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_account_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "account_id" },
                unique: true,
                filter: "account_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_highlight_tags_exactly_one_target",
                schema: "core",
                table: "highlight_tags",
                sql: "num_nonnulls(game_id, account_id, team_id, tournament_id, label_id) = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_highlight_tags_accounts_account_id",
                schema: "core",
                table: "highlight_tags",
                column: "account_id",
                principalSchema: "core",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_highlight_tags_accounts_account_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropIndex(
                name: "IX_highlight_tags_highlight_id_account_id",
                schema: "core",
                table: "highlight_tags");

            migrationBuilder.DropCheckConstraint(
                name: "CK_highlight_tags_exactly_one_target",
                schema: "core",
                table: "highlight_tags");

            // Lossy on purpose: an account tag names a person, not the tournament-scoped player
            // row it came from, so there is nothing faithful to map back to. Drop them instead of
            // inventing a player.
            migrationBuilder.Sql("DELETE FROM core.highlight_tags WHERE account_id IS NOT NULL;");

            migrationBuilder.RenameColumn(
                name: "account_id",
                schema: "core",
                table: "highlight_tags",
                newName: "player_id");

            migrationBuilder.RenameIndex(
                name: "IX_highlight_tags_account_id",
                schema: "core",
                table: "highlight_tags",
                newName: "IX_highlight_tags_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_highlight_tags_highlight_id_player_id",
                schema: "core",
                table: "highlight_tags",
                columns: new[] { "highlight_id", "player_id" },
                unique: true,
                filter: "player_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_highlight_tags_exactly_one_target",
                schema: "core",
                table: "highlight_tags",
                sql: "num_nonnulls(game_id, player_id, team_id, tournament_id, label_id) = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_highlight_tags_players_player_id",
                schema: "core",
                table: "highlight_tags",
                column: "player_id",
                principalSchema: "core",
                principalTable: "players",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
