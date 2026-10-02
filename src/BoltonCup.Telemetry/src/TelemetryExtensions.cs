using System.Reflection;
using BoltonCup.Core;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sentry;

namespace BoltonCup.Telemetry;

/// <summary>
/// The single composition point for telemetry vendors and exporters. Everything else records through
/// <see cref="ITelemetry"/> or <see cref="System.Diagnostics"/>; switching vendors only touches this file.
/// </summary>
/// <remarks>
/// OTLP export is enabled only when <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is set; protocol and sampler come from
/// the standard <c>OTEL_*</c> variables. Collector auth headers come from <c>BoltonCup:Telemetry:OtlpHeaders</c>
/// rather than <c>OTEL_EXPORTER_OTLP_HEADERS</c>, which the Sentry exporter would otherwise forward to Sentry.
/// Sentry receives traces over OTLP and error events from Error-level logs when a DSN is configured.
/// </remarks>
public static class TelemetryExtensions
{
    const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";
    const string OtlpHeadersKey = "BoltonCup:Telemetry:OtlpHeaders";
    const string SentryDsnKey = "Sentry:Dsn";
    const string SentryDsnEnvironmentKey = "SENTRY_DSN";

    static readonly string[] InstrumentedSources = ["Npgsql", "Microsoft.AspNetCore.SignalR.Server"];
    static readonly string[] InstrumentedMeters = ["Npgsql", "Microsoft.AspNetCore.Http.Connections"];

    /// <summary>Registers OpenTelemetry tracing, metrics and logging plus the configured exporters.</summary>
    /// <param name="builder">The host being composed.</param>
    /// <param name="serviceName">The <c>service.name</c> resource attribute, e.g. <c>boltoncup-webapi</c>.</param>
    /// <param name="isExpectedException">
    /// Identifies expected (e.g. domain 4xx) exceptions, which are never reported to Sentry as issues.
    /// </param>
    public static WebApplicationBuilder AddBoltonCupTelemetry(
        this WebApplicationBuilder builder,
        string serviceName,
        Func<Exception, bool> isExpectedException)
    {
        var otlpEnabled = !string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointKey]);
        var otlpHeaders = builder.Configuration[OtlpHeadersKey];
        var sentryDsn = FirstNonBlank(builder.Configuration[SentryDsnKey], builder.Configuration[SentryDsnEnvironmentKey]);

        builder.WebHost.UseSentry(options =>
        {
            options.UseOtlp();
            // Sentry's middleware captures any exception an IExceptionHandler saw, including expected domain errors.
            options.SetBeforeSend((sentryEvent, _) =>
                sentryEvent.Exception is { } exception && isExpectedException(exception)
                    ? null
                    : sentryEvent);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: GetServiceVersion())
                .AddAttributes([new("deployment.environment.name", builder.Environment.EnvironmentName)]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(TelemetryNames.Source)
                    .AddSource(InstrumentedSources)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // WebSocket requests (SignalR hubs) would yield one span lasting the whole connection.
                        options.Filter = context => !context.Request.Path.StartsWithSegments("/health")
                                                    && !context.WebSockets.IsWebSocketRequest;
                        // Response-time enrichment runs after authentication, so the user is populated.
                        options.EnrichWithHttpResponse = (activity, response) =>
                            activity.SetTag("enduser.account_id", response.HttpContext.User.FindFirst(BoltonCupClaimTypes.AccountId)?.Value);
                    })
                    .AddHttpClientInstrumentation();

                if (otlpEnabled)
                {
                    tracing.AddOtlpExporter(options => ApplyOtlpHeaders(options, otlpHeaders));
                }

                if (sentryDsn is not null)
                {
                    tracing.AddSentryOtlpExporter(sentryDsn);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(TelemetryNames.Meter)
                    .AddMeter(InstrumentedMeters)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (otlpEnabled)
                {
                    metrics.AddOtlpExporter(options => ApplyOtlpHeaders(options, otlpHeaders));
                }
            })
            .WithLogging(
                logging =>
                {
                    if (otlpEnabled)
                    {
                        logging.AddOtlpExporter(options => ApplyOtlpHeaders(options, otlpHeaders));
                    }
                },
                options =>
                {
                    options.IncludeScopes = true;
                    options.IncludeFormattedMessage = true;
                });

        return builder;
    }

    /// <summary>Routes exceptions from unobserved tasks, which escape the request pipeline, to telemetry.</summary>
    public static WebApplication UseBoltonCupTelemetry(this WebApplication app)
    {
        var telemetry = app.Services.GetRequiredService<ITelemetry>();

        void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args) =>
            telemetry.TrackException(args.Exception, "exception.source", "UnobservedTask");

        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        app.Lifetime.ApplicationStopping.Register(() => TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException);

        return app;
    }

    static void ApplyOtlpHeaders(OtlpExporterOptions options, string? headers)
    {
        if (!string.IsNullOrWhiteSpace(headers))
        {
            options.Headers = headers;
        }
    }

    static string? FirstNonBlank(params string?[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    static string? GetServiceVersion() => Assembly.GetEntryAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
