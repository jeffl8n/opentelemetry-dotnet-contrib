using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Internal;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.AseClient.Implementation;

/// <summary>
/// Helper class to hold common properties used by AseClientDiagnosticListener.
/// </summary>
internal sealed class AseActivitySourceHelper
{
    public const string SapAseDatabaseSystemName = "sap_ase";

    public static readonly Assembly Assembly = typeof(AseActivitySourceHelper).Assembly;
    public static readonly AssemblyName AssemblyName = Assembly.GetName();
    public static readonly string ActivitySourceName = AssemblyName.Name!;
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, Assembly.GetPackageVersion());

    public static TagList GetTagListFromConnectionInfo(string? dataSource, string? databaseName, AseClientTraceInstrumentationOptions options, out string activityName)
    {
        activityName = SapAseDatabaseSystemName;

        var tags = new TagList
        {
            { SemanticConventions.AttributeDbSystem, SapAseDatabaseSystemName },
        };

        if (options.EnableConnectionLevelAttributes && dataSource != null)
        {
            var connectionDetails = AseConnectionDetails.ParseFromDataSource(dataSource);

            if (options.EmitOldAttributes && !string.IsNullOrEmpty(databaseName))
            {
                tags.Add(SemanticConventions.AttributeDbName, databaseName);
                activityName = databaseName!;
            }

            if (options.EmitNewAttributes && !string.IsNullOrEmpty(databaseName))
            {
                var dbNamespace = !string.IsNullOrEmpty(connectionDetails.InstanceName)
                    ? $"{connectionDetails.InstanceName}.{databaseName}"
                    : databaseName!;
                tags.Add(SemanticConventions.AttributeDbNamespace, dbNamespace);
                activityName = dbNamespace;
            }

            var serverAddress = connectionDetails.ServerHostName ?? connectionDetails.ServerIpAddress;
            if (!string.IsNullOrEmpty(serverAddress))
            {
                tags.Add(SemanticConventions.AttributeServerAddress, serverAddress);
                if (connectionDetails.Port.HasValue)
                {
                    tags.Add(SemanticConventions.AttributeServerPort, connectionDetails.Port);
                }

                if (activityName == SapAseDatabaseSystemName)
                {
                    activityName = connectionDetails.Port.HasValue
                        ? $"{serverAddress}:{connectionDetails.Port}"
                        : serverAddress!;
                }
            }

            if (options.EmitOldAttributes && !string.IsNullOrEmpty(connectionDetails.InstanceName))
            {
                tags.Add(SemanticConventions.AttributeDbMsSqlInstanceName, connectionDetails.InstanceName);
            }
        }
        else if (!string.IsNullOrEmpty(databaseName))
        {
            if (options.EmitNewAttributes)
            {
                tags.Add(SemanticConventions.AttributeDbNamespace, databaseName);
            }

            if (options.EmitOldAttributes)
            {
                tags.Add(SemanticConventions.AttributeDbName, databaseName);
            }

            activityName = databaseName!;
        }

        return tags;
    }
}
