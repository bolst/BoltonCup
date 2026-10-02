using System.Diagnostics;
using System.Diagnostics.Metrics;
using BoltonCup.Core;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Application.Telemetry;

/// <summary>
/// <see cref="ITelemetry"/> built purely on <see cref="System.Diagnostics"/> primitives and <see cref="ILogger"/>.
/// Whatever listens to <see cref="TelemetryNames.Source"/>/<see cref="TelemetryNames.Meter"/> decides where data goes.
/// </summary>
/// <remarks>Every member is fail-safe: a misbehaving listener or logger provider never affects the caller.</remarks>
sealed class ActivityTelemetry(ILogger<ActivityTelemetry> _logger) : ITelemetry
{
    static readonly ActivitySource Source = new ActivitySource(TelemetryNames.Source);
    static readonly Meter Meter = new Meter(TelemetryNames.Meter);

    // Attributes are deliberately kept off metrics (they carry IDs) to bound metric cardinality.
    static readonly Counter<long> EventsCounter = Meter.CreateCounter<long>(TelemetryNames.EventsCounter, description: "Tracked application events.");
    static readonly Counter<long> ExceptionsCounter = Meter.CreateCounter<long>(TelemetryNames.ExceptionsCounter, description: "Tracked exceptions.");
    static readonly Histogram<double> OperationDuration = Meter.CreateHistogram(
        TelemetryNames.OperationDuration,
        unit: "s",
        description: "Duration of tracked operations.",
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30, 60] });

    public void TrackEvent(string name, params object?[] attributes) => FailSafe(() =>
    {
        var tags = ToTags(attributes);

        if (Activity.Current is { } activity)
        {
            activity.AddEvent(new ActivityEvent(name, tags: new ActivityTagsCollection(tags)));
        }
        else
        {
            using var standalone = Source.StartActivity(name, ActivityKind.Internal, parentContext: default(ActivityContext), tags);
        }

        EventsCounter.Add(1, new KeyValuePair<string, object?>("event.name", name));

        using (_logger.BeginScope(tags))
        {
            _logger.LogInformation("Event {EventName}", name);
        }
    });

    public IOperationScope StartOperation(string name, params object?[] attributes)
    {
        Activity? activity = null;
        FailSafe(() => activity = Source.StartActivity(name, ActivityKind.Internal, parentContext: default(ActivityContext), ToTags(attributes)));
        return new OperationScope(name, activity, Stopwatch.GetTimestamp());
    }

    public void TrackException(Exception exception, params object?[] attributes) => FailSafe(() =>
    {
        var tags = ToTags(attributes);

        if (Activity.Current is { } activity)
        {
            activity.AddException(exception, new TagList(tags));
            activity.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        }

        RecordException(exception, handled: false);

        using (_logger.BeginScope(tags))
        {
            _logger.LogError(exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);
        }
    });

    // Handled (domain) exception messages can contain personal data, so only the type is recorded.
    public void TrackHandledException(Exception exception, params object?[] attributes) => FailSafe(() =>
    {
        var tags = ToTags(attributes);

        // A distinct event name (not "exception") so backends don't treat expected domain errors as failures.
        Activity.Current?.AddEvent(new ActivityEvent("exception.handled", tags: new ActivityTagsCollection(tags)
        {
            ["exception.type"] = exception.GetType().FullName,
        }));

        RecordException(exception, handled: true);

        using (_logger.BeginScope(tags))
        {
            _logger.LogWarning("Handled exception: {ExceptionType}", exception.GetType().FullName);
        }
    });

    static void RecordException(Exception exception, bool handled) => ExceptionsCounter.Add(1,
        new KeyValuePair<string, object?>("exception.type", exception.GetType().Name),
        new KeyValuePair<string, object?>("handled", handled));

    static KeyValuePair<string, object?>[] ToTags(object?[]? keyValues)
    {
        if (keyValues is null || keyValues.Length == 0)
        {
            return [];
        }

        var tags = new List<KeyValuePair<string, object?>>(keyValues.Length / 2 + 1);
        var malformed = keyValues.Length % 2 != 0;

        for (var i = 0; i + 1 < keyValues.Length; i += 2)
        {
            if (keyValues[i] is string key)
            {
                tags.Add(new KeyValuePair<string, object?>(key, keyValues[i + 1]));
            }
            else
            {
                malformed = true;
            }
        }

        if (malformed)
        {
            tags.Add(new KeyValuePair<string, object?>(TelemetryNames.MalformedAttributes, true));
        }

        return tags.ToArray();
    }

    static void FailSafe(Action record)
    {
        try
        {
            record();
        }
        catch
        {
            // Telemetry must never break business logic; there is nowhere safe to report this failure.
        }
    }

    sealed class OperationScope(string _name, Activity? _activity, long _startTimestamp) : IOperationScope
    {
        bool _failed;
        bool _disposed;

        public void Fail(Exception exception)
        {
            _failed = true;
            FailSafe(() =>
            {
                _activity?.AddException(exception);
                _activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            });
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            FailSafe(() =>
            {
                OperationDuration.Record(Stopwatch.GetElapsedTime(_startTimestamp).TotalSeconds,
                    new KeyValuePair<string, object?>("operation.name", _name),
                    new KeyValuePair<string, object?>("outcome", _failed ? "error" : "success"));
                _activity?.Dispose();
            });
        }
    }
}
