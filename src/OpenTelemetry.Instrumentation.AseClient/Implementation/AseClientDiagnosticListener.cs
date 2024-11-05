using System.Data;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.AseClient.Implementation;

internal sealed class AseClientDiagnosticListener : ListenerHandler
{
    public const string AseClientBeforeExecuteCommand = "AdoNetCore.AseClient.WriteCommandBefore";
    public const string AseClientAfterExecuteCommand = "AdoNetCore.AseClient.WriteCommandAfter";
    public const string AseClientWriteCommandError = "AdoNetCore.AseClient.WriteCommandError";

    private readonly PropertyFetcher<object> commandFetcher = new("Command");
    private readonly PropertyFetcher<object> connectionFetcher = new("Connection");
    private readonly PropertyFetcher<string> dataSourceFetcher = new("DataSource");
    private readonly PropertyFetcher<string> databaseFetcher = new("Database");
    private readonly PropertyFetcher<CommandType> commandTypeFetcher = new("CommandType");
    private readonly PropertyFetcher<object> commandTextFetcher = new("CommandText");
    private readonly PropertyFetcher<Exception> exceptionFetcher = new("Exception");
    private readonly PropertyFetcher<int> exceptionNumberFetcher = new("Number");
    private readonly AseClientTraceInstrumentationOptions options;

    public AseClientDiagnosticListener(string sourceName, AseClientTraceInstrumentationOptions? options)
        : base(sourceName)
    {
        this.options = options ?? new AseClientTraceInstrumentationOptions();
    }

    public override bool SupportsNullActivity => true;

    public override void OnEventWritten(string name, object? payload)
    {
        var activity = Activity.Current;
        switch (name)
        {
            case AseClientBeforeExecuteCommand:
                {
                    _ = this.commandFetcher.TryFetch(payload, out var command);
                    if (command == null)
                    {
                        AseClientInstrumentationEventSource.Log.NullPayload(nameof(AseClientDiagnosticListener), name);
                        return;
                    }

                    _ = this.connectionFetcher.TryFetch(command, out var connection);
                    _ = this.databaseFetcher.TryFetch(connection, out var databaseName);
                    _ = this.dataSourceFetcher.TryFetch(connection, out var dataSource);

                    var startTags = AseActivitySourceHelper.GetTagListFromConnectionInfo(dataSource, databaseName, this.options, out var activityName);
                    activity = AseActivitySourceHelper.ActivitySource.StartActivity(
                        activityName,
                        ActivityKind.Client,
                        default(ActivityContext),
                        startTags);

                    if (activity == null)
                    {
                        // There is no listener or it decided not to sample the current request.
                        return;
                    }

                    if (activity.IsAllDataRequested)
                    {
                        try
                        {
                            if (this.options.Filter?.Invoke(command) == false)
                            {
                                AseClientInstrumentationEventSource.Log.CommandIsFilteredOut(activity.OperationName);
                                activity.IsAllDataRequested = false;
                                activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            AseClientInstrumentationEventSource.Log.CommandFilterException(ex);
                            activity.IsAllDataRequested = false;
                            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                            return;
                        }

                        _ = this.commandTextFetcher.TryFetch(command, out var commandText);

                        if (this.commandTypeFetcher.TryFetch(command, out CommandType commandType))
                        {
                            switch (commandType)
                            {
                                case CommandType.StoredProcedure:
                                    if (this.options.EmitOldAttributes)
                                    {
                                        activity.SetTag(SemanticConventions.AttributeDbStatement, commandText);
                                    }

                                    if (this.options.EmitNewAttributes)
                                    {
                                        activity.SetTag(SemanticConventions.AttributeDbOperationName, "EXECUTE");
                                        activity.SetTag(SemanticConventions.AttributeDbCollectionName, commandText);
                                        activity.SetTag(SemanticConventions.AttributeDbQueryText, commandText);
                                    }

                                    break;

                                case CommandType.Text:
                                    if (this.options.SetDbStatementForText)
                                    {
                                        if (this.options.EmitOldAttributes)
                                        {
                                            activity.SetTag(SemanticConventions.AttributeDbStatement, commandText);
                                        }

                                        if (this.options.EmitNewAttributes)
                                        {
                                            activity.SetTag(SemanticConventions.AttributeDbQueryText, commandText);
                                        }
                                    }

                                    break;

                                case CommandType.TableDirect:
                                    break;
                            }
                        }

                        try
                        {
                            this.options.Enrich?.Invoke(activity, "OnCustom", command);
                        }
                        catch (Exception ex)
                        {
                            AseClientInstrumentationEventSource.Log.EnrichmentException(ex);
                        }
                    }
                }

                break;
            case AseClientAfterExecuteCommand:
                {
                    if (activity == null)
                    {
                        AseClientInstrumentationEventSource.Log.NullActivity(name);
                        return;
                    }

                    if (activity.Source != AseActivitySourceHelper.ActivitySource)
                    {
                        return;
                    }

                    activity.Stop();
                }

                break;
            case AseClientWriteCommandError:
                {
                    if (activity == null)
                    {
                        AseClientInstrumentationEventSource.Log.NullActivity(name);
                        return;
                    }

                    if (activity.Source != AseActivitySourceHelper.ActivitySource)
                    {
                        return;
                    }

                    try
                    {
                        if (activity.IsAllDataRequested)
                        {
                            if (this.exceptionFetcher.TryFetch(payload, out Exception? exception) && exception != null)
                            {
                                activity.AddTag(SemanticConventions.AttributeErrorType, exception.GetType().FullName);

                                if (this.exceptionNumberFetcher.TryFetch(exception, out var exceptionNumber))
                                {
                                    activity.AddTag(SemanticConventions.AttributeDbResponseStatusCode, exceptionNumber.ToString(CultureInfo.InvariantCulture));
                                }

                                activity.SetStatus(ActivityStatusCode.Error, exception.Message);

                                if (this.options.RecordException)
                                {
                                    activity.RecordException(exception);
                                }
                            }
                            else
                            {
                                AseClientInstrumentationEventSource.Log.NullPayload(nameof(AseClientDiagnosticListener), name);
                            }
                        }
                    }
                    finally
                    {
                        activity.Stop();
                    }
                }

                break;
        }
    }
}
