namespace BoltonCup.Core;

/// <summary>
/// Vendor-neutral telemetry facade. Application code records events, operations and exceptions here;
/// exporter/vendor choices live solely in the WebAPI's telemetry composition.
/// </summary>
/// <remarks>
/// <para>
/// Attributes are passed as alternating keys and values:
/// <c>TrackEvent("draft.player_picked", "draft.id", draftId, "player.id", playerId)</c>.
/// Keys MUST be strings. A malformed list (odd length or non-string key) keeps its well-formed pairs and is
/// flagged with <see cref="TelemetryNames.MalformedAttributes"/> rather than thrown.
/// </para>
/// <para>
/// Event and operation names become metric dimensions: they MUST be constants, never interpolated with IDs.
/// Attributes MUST carry IDs only, never names, emails or phone numbers.
/// </para>
/// </remarks>
public interface ITelemetry
{
    /// <summary>Records a named business or user-action event with optional attributes.</summary>
    void TrackEvent(string name, params object?[] attributes);

    /// <summary>Starts a timed operation (span + duration metric) that ends when the scope is disposed.</summary>
    IOperationScope StartOperation(string name, params object?[] attributes);

    /// <summary>Records an unexpected exception as an error.</summary>
    void TrackException(Exception exception, params object?[] attributes);

    /// <summary>Records an expected, handled exception (e.g. a domain 4xx) without flagging it as an error.</summary>
    void TrackHandledException(Exception exception, params object?[] attributes);
}
