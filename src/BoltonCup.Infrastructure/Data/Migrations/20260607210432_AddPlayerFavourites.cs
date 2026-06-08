using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_favourites",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    draft_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_favourites", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_favourites_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_favourites_drafts_draft_id",
                        column: x => x.draft_id,
                        principalSchema: "core",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_favourites_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "core",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_player_favourites_account_id_draft_id",
                schema: "core",
                table: "player_favourites",
                columns: new[] { "account_id", "draft_id" });

            migrationBuilder.CreateIndex(
                name: "IX_player_favourites_account_id_draft_id_player_id",
                schema: "core",
                table: "player_favourites",
                columns: new[] { "account_id", "draft_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_favourites_draft_id",
                schema: "core",
                table: "player_favourites",
                column: "draft_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_favourites_player_id",
                schema: "core",
                table: "player_favourites",
                column: "player_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_favourites",
                schema: "core");
        }
    }
}
