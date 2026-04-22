using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gallery_id",
                schema: "core",
                table: "tournaments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "galleries",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_galleries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tournaments_gallery_id",
                schema: "core",
                table: "tournaments",
                column: "gallery_id");

            migrationBuilder.CreateIndex(
                name: "IX_galleries_title",
                schema: "core",
                table: "galleries",
                column: "title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tournaments_galleries_gallery_id",
                schema: "core",
                table: "tournaments",
                column: "gallery_id",
                principalSchema: "core",
                principalTable: "galleries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tournaments_galleries_gallery_id",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropTable(
                name: "galleries",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_tournaments_gallery_id",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropColumn(
                name: "gallery_id",
                schema: "core",
                table: "tournaments");
        }
    }
}
