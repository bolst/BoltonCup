using BoltonCup.Persistence.Migrations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace BoltonCup.Application.Tests.Persistence;

// Guards the hand-ordered backfill in AddFranchiseSlugs: the NOT NULL change and unique index must come after every
// franchise has a distinct slug, or the migration fails on existing rows.
public class FranchiseSlugMigrationTests
{
    static readonly IReadOnlyList<MigrationOperation> Operations = new AddFranchiseSlugs().UpOperations;

    static int IndexOf(Func<MigrationOperation, bool> predicate, string because)
    {
        var index = Operations.ToList().FindIndex(o => predicate(o));
        index.Should().BeGreaterThanOrEqualTo(0, because);
        return index;
    }

    static bool IsSql(MigrationOperation operation, string fragment) => operation is SqlOperation sql && sql.Sql.Contains(fragment, StringComparison.Ordinal);

    [Fact]
    public void Up_RunsBackfillBetweenNullableColumnAndConstraints()
    {
        var addColumn = IndexOf(
            o => o is AddColumnOperation { Table: "franchises", Name: "slug", IsNullable: true },
            "slug is added as nullable");
        var backfill = IndexOf(
            o => IsSql(o, "UPDATE core.franchises") && IsSql(o, "row_number() OVER (PARTITION BY base ORDER BY id)"),
            "slugs are backfilled and de-duplicated");
        var guard = IndexOf(o => IsSql(o, "DO $$") && IsSql(o, "RAISE EXCEPTION"), "the backfill is guarded");
        var alter = IndexOf(
            o => o is AlterColumnOperation { Table: "franchises", Name: "slug", IsNullable: false } alter
                && alter.OldColumn.IsNullable,
            "slug becomes NOT NULL");
        var index = IndexOf(
            o => o is CreateIndexOperation { Table: "franchises", Name: "IX_franchises_slug", IsUnique: true },
            "slug is uniquely indexed");

        new[] { addColumn, backfill, guard, alter, index }.Should().BeInAscendingOrder();
        guard.Should().BeLessThan(alter);
    }

    [Fact]
    public void Up_BackfillMatchesSlugGeneratorLimits()
    {
        var backfill = Operations.OfType<SqlOperation>().Single(o => o.Sql.Contains("UPDATE core.franchises", StringComparison.Ordinal));

        backfill.Sql.Should().Contain("80").And.Contain("'franchise'").And.Contain("base || '-' || numbered.n");
    }
}