using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BoltonCup.Application.Telemetry;
using BoltonCup.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BoltonCup.Application.Tests.Telemetry;

// Listeners are process-wide, so every assertion filters by a name unique to its test.
public sealed class ActivityTelemetryTests : IDisposable
{
    readonly ActivityTelemetry _telemetry = new ActivityTelemetry(NullLogger<ActivityTelemetry>.Instance);
    readonly ActivityListener _activityListener;
    readonly MeterListener _meterListener = new MeterListener();
    readonly ConcurrentQueue<Activity> _stoppedActivities = new ConcurrentQueue<Activity>();
    readonly ConcurrentQueue<Measurement> _measurements = new ConcurrentQueue<Measurement>();

    sealed record Measurement(string Instrument, double Value, IReadOnlyDictionary<string, object?> Tags);

    sealed class TelemetryTestException(string message) : Exception(message);

    public ActivityTelemetryTests()
    {
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryNames.Source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = _stoppedActivities.Enqueue,
        };
        ActivitySource.AddActivityListener(_activityListener);

        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == TelemetryNames.Meter)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _meterListener.SetMeasurementEventCallback<long>((instrument, value, tags, _) => Record(instrument, value, tags));
        _meterListener.SetMeasurementEventCallback<double>((instrument, value, tags, _) => Record(instrument, value, tags));
        _meterListener.Start();
    }

    public void Dispose()
    {
        _activityListener.Dispose();
        _meterListener.Dispose();
    }

    void Record(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        _measurements.Enqueue(new Measurement(instrument.Name, value, tags.ToArray().ToDictionary(t => t.Key, t => t.Value)));

    static string UniqueName() => $"test.{Guid.NewGuid():N}";

    [Fact]
    public void TrackEvent_WithAmbientActivity_AddsEventAndIncrementsCounter()
    {
        var name = UniqueName();
        using var parent = new Activity("parent").Start();

        _telemetry.TrackEvent(name, "draft.id", 7);

        parent.Events.Should().ContainSingle(e => e.Name == name)
            .Which.Tags.Should().Contain(new KeyValuePair<string, object?>("draft.id", 7));
        _measurements.Should().ContainSingle(m =>
            m.Instrument == TelemetryNames.EventsCounter && Equals(m.Tags["event.name"], name) && m.Value == 1);
    }

    [Fact]
    public void TrackEvent_WithMalformedAttributes_KeepsValidPairsAndFlagsMalformed()
    {
        var name = UniqueName();
        using var parent = new Activity("parent").Start();

        _telemetry.TrackEvent(name, "draft.id", 7, 42, "orphan", "dangling");

        var tags = parent.Events.Should().ContainSingle(e => e.Name == name).Subject.Tags.ToList();
        tags.Should().Contain(new KeyValuePair<string, object?>("draft.id", 7));
        tags.Should().Contain(new KeyValuePair<string, object?>(TelemetryNames.MalformedAttributes, true));
        tags.Should().HaveCount(2);
    }

    [Fact]
    public void TrackEvent_WithoutAmbientActivity_EmitsStandaloneActivity()
    {
        var name = UniqueName();
        Activity.Current = null;

        _telemetry.TrackEvent(name, "trade.id", 3);

        _stoppedActivities.Should().ContainSingle(a => a.OperationName == name)
            .Which.GetTagItem("trade.id").Should().Be(3);
    }

    [Fact]
    public void StartOperation_OnDispose_StopsActivityAndRecordsSuccessDuration()
    {
        var name = UniqueName();

        using (_telemetry.StartOperation(name, "draft.id", 1))
        {
        }

        _stoppedActivities.Should().ContainSingle(a => a.OperationName == name)
            .Which.Status.Should().Be(ActivityStatusCode.Unset);
        _measurements.Should().ContainSingle(m =>
            m.Instrument == TelemetryNames.OperationDuration && Equals(m.Tags["operation.name"], name))
            .Which.Tags["outcome"].Should().Be("success");
    }

    [Fact]
    public void StartOperation_WithAmbientActivity_NestsUnderIt()
    {
        var name = UniqueName();
        using var parent = new Activity("parent").Start();

        using (_telemetry.StartOperation(name))
        {
        }

        _stoppedActivities.Should().ContainSingle(a => a.OperationName == name)
            .Which.ParentSpanId.Should().Be(parent.SpanId);
    }

    [Fact]
    public void StartOperation_WhenFailed_MarksActivityErrorAndOutcomeError()
    {
        var name = UniqueName();

        using (var operation = _telemetry.StartOperation(name))
        {
            operation.Fail(new TelemetryTestException("boom"));
        }

        var activity = _stoppedActivities.Should().ContainSingle(a => a.OperationName == name).Subject;
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.Events.Should().Contain(e => e.Name == "exception");
        _measurements.Should().ContainSingle(m =>
            m.Instrument == TelemetryNames.OperationDuration && Equals(m.Tags["operation.name"], name))
            .Which.Tags["outcome"].Should().Be("error");
    }

    [Fact]
    public async Task MeasureAsync_WhenOperationThrows_MarksFailureAndRethrows()
    {
        var name = UniqueName();

        var act = () => _telemetry.MeasureAsync(name, () => Task.FromException(new TelemetryTestException("boom")));

        await act.Should().ThrowAsync<TelemetryTestException>();
        _measurements.Should().ContainSingle(m =>
            m.Instrument == TelemetryNames.OperationDuration && Equals(m.Tags["operation.name"], name))
            .Which.Tags["outcome"].Should().Be("error");
    }

    [Fact]
    public void TrackException_SetsErrorStatusAndIncrementsUnhandledCounter()
    {
        using var parent = new Activity("parent").Start();

        _telemetry.TrackException(new TelemetryTestException("boom"), "error.type", "unexpected");

        parent.Status.Should().Be(ActivityStatusCode.Error);
        parent.Events.Should().ContainSingle(e => e.Name == "exception")
            .Which.Tags.Should().Contain(new KeyValuePair<string, object?>("error.type", "unexpected"));
        _measurements.Should().Contain(m =>
            m.Instrument == TelemetryNames.ExceptionsCounter
            && Equals(m.Tags["exception.type"], nameof(TelemetryTestException))
            && Equals(m.Tags["handled"], false));
    }

    [Fact]
    public void TrackHandledException_LeavesStatusUnsetAndIncrementsHandledCounter()
    {
        using var parent = new Activity("parent").Start();

        _telemetry.TrackHandledException(new TelemetryTestException("not found"));

        parent.Status.Should().Be(ActivityStatusCode.Unset);
        parent.Events.Should().ContainSingle(e => e.Name == "exception.handled");
        parent.Events.Should().NotContain(e => e.Name == "exception");
        parent.Events.Single(e => e.Name == "exception.handled").Tags
            .Should().NotContain(t => t.Value is string && ((string)t.Value!).Contains("not found"));
        _measurements.Should().Contain(m =>
            m.Instrument == TelemetryNames.ExceptionsCounter
            && Equals(m.Tags["exception.type"], nameof(TelemetryTestException))
            && Equals(m.Tags["handled"], true));
    }
}
