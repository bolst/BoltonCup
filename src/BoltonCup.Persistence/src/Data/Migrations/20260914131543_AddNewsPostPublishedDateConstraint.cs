using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsPostPublishedDateConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_news_posts_published_has_date",
                schema: "core",
                table: "news_posts",
                sql: "NOT is_published OR published_at IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_news_posts_published_has_date",
                schema: "core",
                table: "news_posts");
        }
    }
}
