using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFranchises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "franchises",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_short = table.Column<string>(type: "text", nullable: false),
                    abbreviation = table.Column<string>(type: "text", nullable: false),
                    logo_key = table.Column<string>(type: "text", nullable: true),
                    banner_key = table.Column<string>(type: "text", nullable: true),
                    primary_hex = table.Column<string>(type: "text", nullable: false),
                    secondary_hex = table.Column<string>(type: "text", nullable: false),
                    tertiary_hex = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_franchises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "franchise_owners",
                schema: "core",
                columns: table => new
                {
                    franchise_id = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_franchise_owners", x => new { x.franchise_id, x.account_id });
                    table.ForeignKey(
                        name: "FK_franchise_owners_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_franchise_owners_franchises_franchise_id",
                        column: x => x.franchise_id,
                        principalSchema: "core",
                        principalTable: "franchises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Hand-ordered backfill: add the column as nullable, give every existing team a franchise with the same id
            // and brand, then make it NOT NULL. Adding it NOT NULL up front would fail the FK on existing rows.
            migrationBuilder.AddColumn<int>(
                name: "franchise_id",
                schema: "core",
                table: "teams",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO core.franchises (id, name, name_short, abbreviation, logo_key, banner_key, primary_hex, secondary_hex, tertiary_hex)
                SELECT id, name, name_short, abbreviation, logo_key, banner_key, primary_hex, secondary_hex, tertiary_hex
                FROM core.teams;
                """);

            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('core.franchises', 'id'), COALESCE((SELECT MAX(id) FROM core.franchises), 0) + 1, false);
                """);

            migrationBuilder.Sql("""
                UPDATE core.teams SET franchise_id = id;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM core.teams WHERE franchise_id IS NULL) THEN
                        RAISE EXCEPTION 'franchise backfill incomplete';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "franchise_id",
                schema: "core",
                table: "teams",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_franchise_id",
                schema: "core",
                table: "teams",
                column: "franchise_id");

            migrationBuilder.CreateIndex(
                name: "IX_franchise_owners_account_id",
                schema: "core",
                table: "franchise_owners",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_franchises_franchise_id",
                schema: "core",
                table: "teams",
                column: "franchise_id",
                principalSchema: "core",
                principalTable: "franchises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_franchises_franchise_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropTable(
                name: "franchise_owners",
                schema: "core");

            migrationBuilder.DropTable(
                name: "franchises",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_teams_franchise_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "franchise_id",
                schema: "core",
                table: "teams");
        }
    }
}
