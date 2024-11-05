using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AseClient;
using OpenTelemetry.Instrumentation.AseClient.Implementation;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Trace;

/// <summary>
/// Extension methods to simplify registering of dependency instrumentation.
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Enables AseClient instrumentation.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddAseClientInstrumentation(this TracerProviderBuilder builder)
        => AddAseClientInstrumentation(builder, name: null, configureAseClientTraceInstrumentationOptions: null);

    /// <summary>
    /// Enables AseClient instrumentation.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <param name="configureAseClientTraceInstrumentationOptions">Callback action for configuring <see cref="AseClientTraceInstrumentationOptions"/>.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddAseClientInstrumentation(
        this TracerProviderBuilder builder,
        Action<AseClientTraceInstrumentationOptions> configureAseClientTraceInstrumentationOptions)
        => AddAseClientInstrumentation(builder, name: null, configureAseClientTraceInstrumentationOptions);

    /// <summary>
    /// Enables AseClient instrumentation.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <param name="name">Name which is used when retrieving options.</param>
    /// <param name="configureAseClientTraceInstrumentationOptions">Callback action for configuring <see cref="AseClientTraceInstrumentationOptions"/>.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddAseClientInstrumentation(
        this TracerProviderBuilder builder,
        string? name,
        Action<AseClientTraceInstrumentationOptions>? configureAseClientTraceInstrumentationOptions)
    {
        Guard.ThrowIfNull(builder);

        name ??= Options.DefaultName;

        if (configureAseClientTraceInstrumentationOptions != null)
        {
            builder.ConfigureServices(services => services.Configure(name, configureAseClientTraceInstrumentationOptions));
        }

        builder.AddInstrumentation(sp =>
        {
            var aseOptions = sp.GetRequiredService<IOptionsMonitor<AseClientTraceInstrumentationOptions>>().Get(name);

            return new AseClientInstrumentation(aseOptions);
        });

        builder.AddSource(AseActivitySourceHelper.ActivitySourceName);

        return builder;
    }
}
