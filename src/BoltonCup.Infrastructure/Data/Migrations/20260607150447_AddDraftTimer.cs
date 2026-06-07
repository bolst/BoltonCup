using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "seconds_per_pick",
                schema: "core",
                table: "drafts",
                type: "integer",
                nullable: false,
                defaultValue: 120);

            migrationBuilder.AddColumn<DateTime>(
                name: "clock_started_at",
                schema: "core",
                table: "draft_picks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "seconds_per_pick",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropColumn(
                name: "clock_started_at",
                schema: "core",
                table: "draft_picks");
        }
    }
}
