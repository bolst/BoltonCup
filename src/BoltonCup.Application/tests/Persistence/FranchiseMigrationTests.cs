using BoltonCup.Persistence.Migrations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace BoltonCup.Application.Tests.Persistence;

// Guards the hand-ordered backfill in AddFranchises: the NOT NULL change, index and FK must come after every team
// has a franchise, or the migration fails on existing rows.
public class FranchiseMigrationTests
{
    static readonly IReadOnlyList<MigrationOperation> Operations = new AddFranchises().UpOperations;

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
        var createFranchises = IndexOf(o => o is CreateTableOperation { Name: "franchises" }, "franchises is created");
        var createOwners = IndexOf(o => o is CreateTableOperation { Name: "franchise_owners" }, "franchise_owners is created");
        var addColumn = IndexOf(
            o => o is AddColumnOperation { Table: "teams", Name: "franchise_id", IsNullable: true },
            "franchise_id is added as nullable");
        var insert = IndexOf(
            o => IsSql(o, "INSERT INTO core.franchises") && IsSql(o, "FROM core.teams"),
            "franchises are backfilled from teams");
        var setval = IndexOf(o => IsSql(o, "setval(pg_get_serial_sequence('core.franchises'"), "the identity sequence is advanced");
        var update = IndexOf(o => IsSql(o, "UPDATE core.teams SET franchise_id = id"), "teams point at their franchise");
        var guard = IndexOf(o => IsSql(o, "DO $$") && IsSql(o, "RAISE EXCEPTION"), "the backfill is guarded");
        var alter = IndexOf(
            o => o is AlterColumnOperation { Table: "teams", Name: "franchise_id", IsNullable: false } alter
                && alter.OldColumn.IsNullable,
            "franchise_id becomes NOT NULL");
        var index = IndexOf(o => o is CreateIndexOperation { Name: "IX_teams_franchise_id" }, "the FK column is indexed");
        var foreignKey = IndexOf(
            o => o is AddForeignKeyOperation { Name: "FK_teams_franchises_franchise_id", OnDelete: ReferentialAction.Restrict },
            "the restrict FK is added");

        new[] { createFranchises, createOwners, addColumn, insert, setval, update, guard, alter, index, foreignKey }
            .Should().BeInAscendingOrder();
        createFranchises.Should().BeLessThan(createOwners);
        guard.Should().BeLessThan(alter);
    }

    [Fact]
    public void Up_AddsNoUniqueIndexOnFranchiseAndTournament()
    {
        Operations.OfType<CreateIndexOperation>()
            .Where(o => o.Table == "teams" && o.Columns.Contains("franchise_id"))
            .Should().OnlyContain(o => !o.IsUnique && o.Columns.Length == 1);
    }
}