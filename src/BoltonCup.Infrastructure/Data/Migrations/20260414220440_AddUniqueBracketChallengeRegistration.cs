using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueBracketChallengeRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bracket_challenge_registrations_bracket_challenge_event_id",
                schema: "core",
                table: "bracket_challenge_registrations");

            migrationBuilder.CreateIndex(
                name: "IX_bracket_challenge_registrations_bracket_challenge_event_id_~",
                schema: "core",
                table: "bracket_challenge_registrations",
                columns: new[] { "bracket_challenge_event_id", "email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bracket_challenge_registrations_bracket_challenge_event_id_~",
                schema: "core",
                table: "bracket_challenge_registrations");

            migrationBuilder.CreateIndex(
                name: "IX_bracket_challenge_registrations_bracket_challenge_event_id",
                schema: "core",
                table: "bracket_challenge_registrations",
                column: "bracket_challenge_event_id");
        }
    }
}
