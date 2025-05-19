using Dapper;
using Npgsql;
using System.Text.Json;

namespace BoltonCup.Shared.Data;

public abstract class DapperBase
{

    private const int BATCH_SIZE = 200;

    protected readonly string _connectionString;

    public DapperBase(string connectionString)
    {
        _connectionString = connectionString;
    }



    /// <summary>
    /// Connects and queries the database using the set connection string.
    /// </summary>
    /// <typeparam name="T">The type of data being queried.</typeparam>
    /// <param name="query">The SQL query to be performed.</param>
    /// <param name="param">Parameters to use in <paramref name="query" />.</param>
    /// <returns>A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case-insensitive). </returns>
    protected async Task<IEnumerable<T>> QueryDbAsync<T>(string query, object? param = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<T>(query, param);
    }



    /// <summary>
    /// Connects and executes a single-row query on the database using the set connection string.
    /// </summary>
    /// <typeparam name="T">The type of data being queried.</typeparam>
    /// <param name="query">The SQL query to be performed.</param>
    /// <param name="param">Parameters to use in <paramref name="query" />.</param>
    /// <returns>An object of <typeparamref name="T"/>, unless it does not exist then <c>null</c>.</returns>
    protected async Task<T?> QueryDbSingleAsync<T>(string query, object? param = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<T>(query, param);
    }



    /// <summary>
    /// Connects and executes a command on the database using the set connection string.
    /// </summary>
    /// <param name="query">The SQL query to be performed.</param>
    /// <param name="param">Parameters to use in <paramref name="query" />.</param>
    /// <returns>The number of rows affected.</returns>
    protected async Task<int> ExecuteSqlAsync(string query, object? param = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteAsync(query, param);
    }



    /// <summary>
    /// Connects and batch inserts data into the database using the set connection string.
    /// </summary>
    /// <typeparam name="T">The type of data being inserted.</typeparam>
    /// <param name="query">The SQL query to be performed for each batch.</param>
    /// <param name="data">The data to be inserted using <paramref name="query"/>.</param>
    /// <param name="param">Parameters to use in <paramref name="query" />.</param>
    /// <returns></returns>
    protected async Task<BatchInsertResult> BatchInsertAsync<T>(string query, IEnumerable<T> data, object? param = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var retval = new BatchInsertResult();

        if (data is null || !data.Any()) return retval;

        int numRecords = data.Count();
        for (int i = 0; i < numRecords; i += BATCH_SIZE)
        {
            var batch = data.Skip(i).Take(BATCH_SIZE).ToList();

            // build parameters
            List<DynamicParameters> parameters;
            if (param is null)
            {
                parameters = batch.Select(b => new DynamicParameters(b)).ToList();
            }
            else
            {
                parameters = batch.Select(b =>
                {
                    var p = new DynamicParameters(b);
                    p.AddDynamicParams(param);
                    return p;
                }).ToList();
            }

            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    int result = await connection.ExecuteAsync(query, parameters, transaction);
                    retval.SuccessfulInserts += result;

                    transaction.Commit();
                }
                catch (PostgresException pgEx)
                {
                    transaction.Rollback();

                    Console.WriteLine($"Batch insert failed: {pgEx.Message}");

                    // fall back to individual inserts for this batch
                    foreach (var item in batch)
                    {
                        try
                        {
                            var parameter = new DynamicParameters(item);
                            if (param is not null) parameter.AddDynamicParams(param);

                            await connection.ExecuteAsync(query, parameter);
                            retval.SuccessfulInserts++;
                        }
                        catch (Exception itemEx)
                        {
                            retval.FailedInserts++;
                            retval.Errors.Add($"Error inserting record for {item}: {itemEx.Message}");
                            Console.WriteLine($"Insert error: {itemEx.Message} for {JsonSerializer.Serialize(item)}");
                        }
                    }
                }
            }
        }

        return retval;
    }



    /// <summary>
    /// Result object for <see cref="BatchInsertAsync{T}(string, IEnumerable{T}, object?)"/>
    /// </summary>
    public class BatchInsertResult
    {
        public int SuccessfulInserts { get; set; }
        public int FailedInserts { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
