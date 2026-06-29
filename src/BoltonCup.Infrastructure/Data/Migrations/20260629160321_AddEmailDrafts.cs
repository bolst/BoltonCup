using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoltonCup.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_drafts",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    use_layout = table.Column<bool>(type: "boolean", nullable: false),
                    audience_kind = table.Column<string>(type: "text", nullable: false),
                    tournament_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() AT TIME ZONE 'UTC'"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_drafts", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_drafts",
                schema: "core");
        }
    }
}
