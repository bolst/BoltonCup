using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSnakeRoundPick : Migration
    {
        // round_pick_number historically stored the team's draft slot for snake even rounds. It now
        // stores the true within-round sequence; mirror existing snake even-round picks to match.
        // The transform (teams - round_pick_number + 1) is its own inverse, so Up and Down are identical.
        private const string MirrorSnakeEvenRounds = @"
            UPDATE core.draft_picks dp
            SET round_pick_number = d.teams - dp.round_pick_number + 1
            FROM core.drafts d
            WHERE dp.draft_id = d.id
              AND d.draft_type = 'snake'
              AND dp.round_number % 2 = 0;";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(MirrorSnakeEvenRounds);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(MirrorSnakeEvenRounds);
        }
    }
}
