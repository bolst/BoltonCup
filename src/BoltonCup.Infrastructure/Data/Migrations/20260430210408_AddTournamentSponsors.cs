using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentSponsors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tournament_sponsors",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tournament_id = table.Column<int>(type: "integer", nullable: false),
                    sponsor_name = table.Column<string>(type: "text", nullable: true),
                    logo_key = table.Column<string>(type: "text", nullable: true),
                    website_url = table.Column<string>(type: "text", nullable: true),
                    sort_key = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_sponsors", x => x.id);
                    table.ForeignKey(
                        name: "FK_tournament_sponsors_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tournament_sponsors_tournament_id",
                schema: "core",
                table: "tournament_sponsors",
                column: "tournament_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tournament_sponsors",
                schema: "core");
        }
    }
}
