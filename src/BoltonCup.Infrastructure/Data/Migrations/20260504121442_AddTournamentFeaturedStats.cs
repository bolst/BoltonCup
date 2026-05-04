using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentFeaturedStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "featured_stats_label",
                schema: "core",
                table: "tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_stats_featured",
                schema: "core",
                table: "tournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_tournaments_is_stats_featured",
                schema: "core",
                table: "tournaments",
                column: "is_stats_featured",
                unique: true,
                filter: "is_stats_featured = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tournaments_is_stats_featured",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropColumn(
                name: "featured_stats_label",
                schema: "core",
                table: "tournaments");

            migrationBuilder.DropColumn(
                name: "is_stats_featured",
                schema: "core",
                table: "tournaments");
        }
    }
}
