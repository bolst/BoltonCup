using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceBracketChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "logo",
                schema: "core",
                table: "bracket_challenge_events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "registration_close_date",
                schema: "core",
                table: "bracket_challenge_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terms_of_service_markdown_content",
                schema: "core",
                table: "bracket_challenge_events",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "logo",
                schema: "core",
                table: "bracket_challenge_events");

            migrationBuilder.DropColumn(
                name: "registration_close_date",
                schema: "core",
                table: "bracket_challenge_events");

            migrationBuilder.DropColumn(
                name: "terms_of_service_markdown_content",
                schema: "core",
                table: "bracket_challenge_events");
        }
    }
}
