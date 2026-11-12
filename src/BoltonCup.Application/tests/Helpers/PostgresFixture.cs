using BoltonCup.Persistence.Data;
using BoltonCup.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace BoltonCup.Application.Tests.Helpers;

/// <summary>
/// One Postgres container for every test in <see cref="PostgresCollection"/>. For behaviour InMemory can't run:
/// <c>ExecuteUpdate</c>, raw SQL, unique indexes and row locks.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Creates an empty database holding both contexts' tables, and returns its connection string.</summary>
    /// <remarks>Tables come from the current model rather than the migrations, which don't run on a fresh database.</remarks>
    public async Task<string> CreateDatabaseAsync()
    {
        var databaseName = $"test_{Guid.NewGuid():N}";
        await using (var admin = new NpgsqlConnection(_container.GetConnectionString()))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE {databaseName}", admin);
            await create.ExecuteNonQueryAsync();
        }

        var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = databaseName,
        }.ConnectionString;

        await using (var auth = new AuthDbContext(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(connectionString).Options))
        {
            await auth.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
        }

        await using (var app = new BoltonCupDbContext(new DbContextOptionsBuilder<BoltonCupDbContext>().UseNpgsql(connectionString).Options))
        {
            await app.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();

            // The model can't express the key into the auth schema, so run the SQL the migration adds it with.
            var addUserForeignKey = new MoveUserLinkToAccounts().UpOperations
                .OfType<SqlOperation>()
                .Single(operation => operation.Sql.Contains("FOREIGN KEY"));
            await app.Database.ExecuteSqlRawAsync(addUserForeignKey.Sql);
        }

        return connectionString;
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}