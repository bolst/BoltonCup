namespace BoltonCup.Core;

/// <summary>A timed operation started by <see cref="ITelemetry.StartOperation"/>; disposing it ends the operation.</summary>
public interface IOperationScope : IDisposable
{
    /// <summary>Marks the operation as failed with the given exception.</summary>
    void Fail(Exception exception);
}
