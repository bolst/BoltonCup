using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftOwnershipAndVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "draft_owner_account_id",
                schema: "core",
                table: "drafts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_visible",
                schema: "core",
                table: "drafts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_drafts_draft_owner_account_id",
                schema: "core",
                table: "drafts",
                column: "draft_owner_account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_drafts_accounts_draft_owner_account_id",
                schema: "core",
                table: "drafts",
                column: "draft_owner_account_id",
                principalSchema: "core",
                principalTable: "accounts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_drafts_accounts_draft_owner_account_id",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropIndex(
                name: "IX_drafts_draft_owner_account_id",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropColumn(
                name: "draft_owner_account_id",
                schema: "core",
                table: "drafts");

            migrationBuilder.DropColumn(
                name: "is_visible",
                schema: "core",
                table: "drafts");
        }
    }
}
