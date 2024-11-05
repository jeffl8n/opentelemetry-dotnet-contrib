using System.Diagnostics;
using OpenTelemetry.Instrumentation.AseClient.Implementation;

namespace OpenTelemetry.Instrumentation.AseClient;

/// <summary>
/// AseClient instrumentation.
/// </summary>
internal sealed class AseClientInstrumentation : IDisposable
{
    internal const string AseClientDiagnosticListenerName = "AseClientDiagnosticListener";

    private static readonly HashSet<string> DiagnosticSourceEvents = new()
    {
        "AdoNetCore.AseClient.WriteCommandBefore",
        "AdoNetCore.AseClient.WriteCommandAfter",
        "AdoNetCore.AseClient.WriteCommandError",
    };

    private readonly Func<string, object?, object?, bool> isEnabled = (eventName, _, _)
        => DiagnosticSourceEvents.Contains(eventName);

    private readonly DiagnosticSourceSubscriber diagnosticSourceSubscriber;

    /// <summary>
    /// Initializes a new instance of the <see cref="AseClientInstrumentation"/> class.
    /// </summary>
    /// <param name="options">Configuration options for AseClient instrumentation.</param>
    public AseClientInstrumentation(
        AseClientTraceInstrumentationOptions? options = null)
    {
        this.diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(
           name => new AseClientDiagnosticListener(name, options),
           listener => listener.Name == AseClientDiagnosticListenerName,
           this.isEnabled,
           AseClientInstrumentationEventSource.Log.UnknownErrorProcessingEvent);
        this.diagnosticSourceSubscriber.Subscribe();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.diagnosticSourceSubscriber?.Dispose();
    }
}
