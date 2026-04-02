using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountHeightWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "height_feet",
                schema: "core",
                table: "accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height_inches",
                schema: "core",
                table: "accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight",
                schema: "core",
                table: "accounts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "height_feet",
                schema: "core",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "height_inches",
                schema: "core",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "weight",
                schema: "core",
                table: "accounts");
        }
    }
}
