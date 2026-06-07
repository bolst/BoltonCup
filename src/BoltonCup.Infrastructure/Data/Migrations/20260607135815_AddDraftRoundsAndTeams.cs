using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftRoundsAndTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "rounds",
                schema: "core",
                table: "drafts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "teams",
                schema: "core",
                table: "drafts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE core.drafts d SET
                    teams = (SELECT COUNT(*) FROM core.draft_orders o WHERE o.draft_id = d.id),
                    rounds = (SELECT COALESCE(MAX(p.round_number), 0) FROM core.draft_picks p WHERE p.draft_id = d.id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rounds",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropColumn(
                name: "teams",
                schema: "core",
                table: "drafts");
        }
    }
}
