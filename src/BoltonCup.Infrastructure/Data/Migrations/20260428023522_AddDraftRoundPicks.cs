using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftRoundPicks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "round_number",
                schema: "core",
                table: "draft_picks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "round_pick_number",
                schema: "core",
                table: "draft_picks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "round_number",
                schema: "core",
                table: "draft_picks");

            migrationBuilder.DropColumn(
                name: "round_pick_number",
                schema: "core",
                table: "draft_picks");
        }
    }
}
