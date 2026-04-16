using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GameStarEntityBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "core",
                table: "game_stars",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() AT TIME ZONE 'UTC'");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "core",
                table: "game_stars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                schema: "core",
                table: "game_stars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by",
                schema: "core",
                table: "game_stars",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "core",
                table: "game_stars");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "core",
                table: "game_stars");

            migrationBuilder.DropColumn(
                name: "last_modified",
                schema: "core",
                table: "game_stars");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                schema: "core",
                table: "game_stars");
        }
    }
}
