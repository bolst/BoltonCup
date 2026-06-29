using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleTeamGms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "team_general_managers",
                schema: "core",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    tournament_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_general_managers", x => new { x.team_id, x.account_id });
                    table.ForeignKey(
                        name: "FK_team_general_managers_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_general_managers_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "core",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_general_managers_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalSchema: "core",
                        principalTable: "tournaments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_team_general_managers_account_id",
                schema: "core",
                table: "team_general_managers",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_general_managers_tournament_id_account_id",
                schema: "core",
                table: "team_general_managers",
                columns: new[] { "tournament_id", "account_id" },
                unique: true);

            // Preserve existing single-GM assignments (with their tournament) before dropping the column.
            migrationBuilder.Sql(
                "INSERT INTO core.team_general_managers (team_id, account_id, tournament_id) " +
                "SELECT id, gm_account_id, tournament_id FROM core.teams WHERE gm_account_id IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_accounts_gm_account_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_gm_account_id",
                schema: "core",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "gm_account_id",
                schema: "core",
                table: "teams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gm_account_id",
                schema: "core",
                table: "teams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_gm_account_id",
                schema: "core",
                table: "teams",
                column: "gm_account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_accounts_gm_account_id",
                schema: "core",
                table: "teams",
                column: "gm_account_id",
                principalSchema: "core",
                principalTable: "accounts",
                principalColumn: "id");

            // Collapse the many-to-many back to a single GM per team (lowest account id).
            migrationBuilder.Sql(
                "UPDATE core.teams t SET gm_account_id = sub.account_id " +
                "FROM (SELECT team_id, MIN(account_id) AS account_id FROM core.team_general_managers GROUP BY team_id) sub " +
                "WHERE t.id = sub.team_id;");

            migrationBuilder.DropTable(
                name: "team_general_managers",
                schema: "core");
        }
    }
}
