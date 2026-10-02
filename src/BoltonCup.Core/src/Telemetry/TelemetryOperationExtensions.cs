namespace BoltonCup.Core;

public static class TelemetryOperationExtensions
{
    /// <summary>Runs <paramref name="operation"/> as a timed operation, marking it failed if it throws.</summary>
    public static async Task<T> MeasureAsync<T>(
        this ITelemetry telemetry,
        string name,
        Func<Task<T>> operation,
        params object?[] attributes)
    {
        using var scope = telemetry.StartOperation(name, attributes);
        try
        {
            return await operation();
        }
        catch (Exception exception)
        {
            scope.Fail(exception);
            throw;
        }
    }

    /// <inheritdoc cref="MeasureAsync{T}"/>
    public static async Task MeasureAsync(
        this ITelemetry telemetry,
        string name,
        Func<Task> operation,
        params object?[] attributes)
    {
        using var scope = telemetry.StartOperation(name, attributes);
        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            scope.Fail(exception);
            throw;
        }
    }
}
