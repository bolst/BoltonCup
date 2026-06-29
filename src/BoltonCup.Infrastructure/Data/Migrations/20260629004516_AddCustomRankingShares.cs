using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomRankingShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "custom_ranking_shares",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    custom_ranking_id = table.Column<int>(type: "integer", nullable: false),
                    shared_with_account_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_ranking_shares", x => x.id);
                    table.ForeignKey(
                        name: "FK_custom_ranking_shares_accounts_shared_with_account_id",
                        column: x => x.shared_with_account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_custom_ranking_shares_custom_rankings_custom_ranking_id",
                        column: x => x.custom_ranking_id,
                        principalSchema: "core",
                        principalTable: "custom_rankings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_custom_ranking_shares_custom_ranking_id_shared_with_account~",
                schema: "core",
                table: "custom_ranking_shares",
                columns: new[] { "custom_ranking_id", "shared_with_account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_custom_ranking_shares_shared_with_account_id",
                schema: "core",
                table: "custom_ranking_shares",
                column: "shared_with_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custom_ranking_shares",
                schema: "core");
        }
    }
}
