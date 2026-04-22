using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceDraftMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "draft_title",
                schema: "core",
                table: "drafts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                schema: "core",
                table: "drafts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "draft_title",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropColumn(
                name: "start_date",
                schema: "core",
                table: "drafts");
        }
    }
}
