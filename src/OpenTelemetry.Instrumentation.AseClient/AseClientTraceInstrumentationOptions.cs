// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Data;
using System.Diagnostics;

using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;

using static OpenTelemetry.Internal.DatabaseSemanticConventionHelper;

namespace OpenTelemetry.Instrumentation.AseClient;

/// <summary>
/// Options for <see cref="AseClientInstrumentation"/>.
/// </summary>
/// <remarks>
/// For help and examples see: <a href="https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Instrumentation.AseClient/README.md#advanced-configuration" />.
/// </remarks>
public class AseClientTraceInstrumentationOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AseClientTraceInstrumentationOptions"/> class.
    /// </summary>
    public AseClientTraceInstrumentationOptions()
        : this(new ConfigurationBuilder().AddEnvironmentVariables().Build())
    {
    }

    internal AseClientTraceInstrumentationOptions(IConfiguration configuration)
    {
        var databaseSemanticConvention = GetSemanticConventionOptIn(configuration);
        this.EmitOldAttributes = databaseSemanticConvention.HasFlag(DatabaseSemanticConvention.Old);
        this.EmitNewAttributes = databaseSemanticConvention.HasFlag(DatabaseSemanticConvention.New);
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the <see
    /// cref="AseClientInstrumentation"/> should add the text of commands as
    /// the <see cref="SemanticConventions.AttributeDbStatement"/> tag.
    /// Default value: <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>WARNING: SetDbStatementForText will capture the raw
    /// <c>CommandText</c>. Make sure your <c>CommandText</c> property never
    /// contains any sensitive data.</b>
    /// </para>
    /// <para><b>SetDbStatementForText is supported on all runtimes.</b>
    /// <list type="bullet">
    /// <item>On .NET and .NET Core SetDbStatementForText only applies to
    /// <c>AseCommand</c>s with <see cref="CommandType.Text"/>.</item>
    /// <item>On .NET Framework SetDbStatementForText applies to all
    /// <c>AseCommand</c>s regardless of <see cref="CommandType"/>.
    /// <list type="bullet">
    /// <item>When using <c>AdoNetCore.AseClient</c> use
    /// SetDbStatementForText to capture StoredProcedure command
    /// names.</item>
    /// <item>When using <c>AdoNetCore.AseClient</c> use
    /// SetDbStatementForText to capture Text, StoredProcedure, and all
    /// other command text.</item>
    /// </list></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool SetDbStatementForText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the <see
    /// cref="AseClientInstrumentation"/> should parse the DataSource on a
    /// AseConnection into server name, instance name, and/or port
    /// connection-level attribute tags. Default value: <see
    /// langword="true"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>EnableConnectionLevelAttributes is supported on all runtimes.</b>
    /// </para>
    /// <para>
    /// If enabled, AseConnection DataSource will be parsed and the server name will be sent as the
    /// <see cref="SemanticConventions.AttributeServerAddress"/> tag,
    /// the instance name will be sent as the <see cref="SemanticConventions.AttributeDbMsSqlInstanceName"/> tag,
    /// and the port will be sent as the <see cref="SemanticConventions.AttributeServerPort"/> tag if it is not 5000 (the default port).
    /// </para>
    /// </remarks>
    public bool EnableConnectionLevelAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets an action to enrich an <see cref="Activity"/> with the
    /// raw <c>AseCommand</c> object.
    /// </summary>
    /// <remarks>
    /// <para><b>Enrich is only executed on .NET and .NET Core
    /// runtimes.</b></para>
    /// The parameters passed to the enrich action are:
    /// <list type="number">
    /// <item>The <see cref="Activity"/> being enriched.</item>
    /// <item>The name of the event. Currently only <c>"OnCustom"</c> is
    /// used but more events may be added in the future.</item>
    /// <item>The raw <c>AseCommand</c> object from which additional
    /// information can be extracted to enrich the <see
    /// cref="Activity"/>.</item>
    /// </list>
    /// </remarks>
    public Action<Activity, string, object>? Enrich { get; set; }

    /// <summary>
    /// Gets or sets a filter function that determines whether or not to
    /// collect telemetry about a command.
    /// </summary>
    /// <remarks>
    /// <para><b>Filter is only executed on .NET and .NET Core
    /// runtimes.</b></para>
    /// Notes:
    /// <list type="bullet">
    /// <item>The first parameter passed to the filter function is the raw
    /// <c>AseCommand</c> object for the command being executed.</item>
    /// <item>The return value for the filter function is interpreted as:
    /// <list type="bullet">
    /// <item>If filter returns <see langword="true" />, the command is
    /// collected.</item>
    /// <item>If filter returns <see langword="false" /> or throws an
    /// exception the command is NOT collected.</item>
    /// </list></item>
    /// </list>
    /// </remarks>
    public Func<object, bool>? Filter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception will be
    /// recorded as <see cref="ActivityEvent"/> or not. Default value: <see
    /// langword="false"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>RecordException is only supported on .NET and .NET Core
    /// runtimes.</b></para>
    /// <para>For specification details see: <see
    /// href="https://github.com/open-telemetry/semantic-conventions/blob/main/docs/exceptions/exceptions-spans.md"/>.</para>
    /// </remarks>
    public bool RecordException { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the old database attributes should be emitted.
    /// </summary>
    internal bool EmitOldAttributes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the new database attributes should be emitted.
    /// </summary>
    internal bool EmitNewAttributes { get; set; }
}
