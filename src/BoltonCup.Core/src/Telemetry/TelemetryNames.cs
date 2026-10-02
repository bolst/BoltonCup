namespace BoltonCup.Core;

/// <summary>Instrumentation names shared by the telemetry implementation and its exporter configuration.</summary>
public static class TelemetryNames
{
    public const string Source = "BoltonCup";
    public const string Meter = "BoltonCup";

    public const string EventsCounter = "boltoncup.events";
    public const string OperationDuration = "boltoncup.operation.duration";
    public const string ExceptionsCounter = "boltoncup.exceptions";

    /// <summary>Tag added when an attribute list has an odd length or a non-string key.</summary>
    public const string MalformedAttributes = "telemetry.attributes_malformed";
}
