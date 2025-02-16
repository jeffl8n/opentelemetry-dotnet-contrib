// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using OpenTelemetry.Instrumentation.Http.Implementation;

namespace OpenTelemetry.Instrumentation.Http;

/// <summary>
/// HttpClient instrumentation.
/// </summary>
internal sealed class HttpClientInstrumentation : IDisposable
{
    private static readonly HashSet<string> ExcludedDiagnosticSourceEventsNet7OrGreater =
    [
        "System.Net.Http.Request",
        "System.Net.Http.Response",
        "System.Net.Http.HttpRequestOut"
    ];

    private static readonly HashSet<string> ExcludedDiagnosticSourceEvents =
    [
        "System.Net.Http.Request",
        "System.Net.Http.Response"
    ];

    private readonly DiagnosticSourceSubscriber diagnosticSourceSubscriber;

    private readonly Func<string, object?, object?, bool> isEnabled = (eventName, _, _)
        => !ExcludedDiagnosticSourceEvents.Contains(eventName);

    private readonly Func<string, object?, object?, bool> isEnabledNet7OrGreater = (eventName, _, _)
        => !ExcludedDiagnosticSourceEventsNet7OrGreater.Contains(eventName);

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientInstrumentation"/> class.
    /// </summary>
    /// <param name="options">Configuration options for HTTP client instrumentation.</param>
    public HttpClientInstrumentation(HttpClientTraceInstrumentationOptions options)
    {
        // For .NET7.0 activity will be created using activitySource.
        // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http/src/System/Net/Http/DiagnosticsHandler.cs
        // However, in case when activity creation returns null (due to sampling)
        // the framework will fall back to creating activity anyways due to active diagnostic source listener
        // To prevent this, isEnabled is implemented which will return false always
        // so that the sampler's decision is respected.
        this.diagnosticSourceSubscriber = HttpHandlerDiagnosticListener.IsNet7OrGreater
                ? new DiagnosticSourceSubscriber(new HttpHandlerDiagnosticListener(options), this.isEnabledNet7OrGreater, HttpInstrumentationEventSource.Log.UnknownErrorProcessingEvent)
                : new DiagnosticSourceSubscriber(new HttpHandlerDiagnosticListener(options), this.isEnabled, HttpInstrumentationEventSource.Log.UnknownErrorProcessingEvent);

        this.diagnosticSourceSubscriber.Subscribe();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.diagnosticSourceSubscriber?.Dispose();
    }
}
