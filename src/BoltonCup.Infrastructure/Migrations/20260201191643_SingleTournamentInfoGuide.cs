using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SingleTournamentInfoGuide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_info_guides_tournament_id",
                schema: "core",
                table: "info_guides");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "core",
                table: "info_guides",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_info_guides_tournament_id",
                schema: "core",
                table: "info_guides",
                column: "tournament_id",
                unique: true,
                filter: "tournament_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_info_guides_tournament_id",
                schema: "core",
                table: "info_guides");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "core",
                table: "info_guides",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_info_guides_tournament_id",
                schema: "core",
                table: "info_guides",
                column: "tournament_id");
        }
    }
}
