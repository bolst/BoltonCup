using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFranchiseSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered backfill: add the column as nullable, derive a unique slug for every existing franchise,
            // then make it NOT NULL and unique. Adding it NOT NULL up front would give every row the same empty slug.
            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "core",
                table: "franchises",
                type: "text",
                nullable: true);

            // Mirrors SlugGenerator.Generate(name, "franchise"): strip accents, lowercase, collapse every run of
            // non-alphanumerics into '-', trim '-', cap at 80 characters, fall back to 'franchise'. Repeats get
            // SlugGenerator.WithSuffix's '-2', '-3'... in id order, so the oldest franchise keeps the bare slug.
            migrationBuilder.Sql("""
                WITH slugged AS (
                    SELECT id,
                           COALESCE(
                               NULLIF(
                                   rtrim(
                                       left(
                                           btrim(
                                               regexp_replace(
                                                   lower(regexp_replace(normalize(name, NFD), '[\u0300-\u036f]', '', 'g')),
                                                   '[^a-z0-9]+', '-', 'g'),
                                               '-'),
                                           80),
                                       '-'),
                                   ''),
                               'franchise') AS base
                    FROM core.franchises
                ),
                numbered AS (
                    SELECT id, base, row_number() OVER (PARTITION BY base ORDER BY id) AS n
                    FROM slugged
                )
                UPDATE core.franchises AS f
                SET slug = CASE WHEN numbered.n = 1 THEN numbered.base ELSE numbered.base || '-' || numbered.n END
                FROM numbered
                WHERE numbered.id = f.id;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM core.franchises WHERE slug IS NULL OR slug = '') THEN
                        RAISE EXCEPTION 'franchise slug backfill incomplete';
                    END IF;
                    IF EXISTS (SELECT 1 FROM core.franchises GROUP BY slug HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'franchise slug backfill produced duplicates';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "slug",
                schema: "core",
                table: "franchises",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_franchises_slug",
                schema: "core",
                table: "franchises",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_franchises_slug",
                schema: "core",
                table: "franchises");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "core",
                table: "franchises");
        }
    }
}
