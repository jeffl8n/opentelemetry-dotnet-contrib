# AseClient Instrumentation for OpenTelemetry

| Status        |           |
| ------------- |-----------|
| Stability     |  [Beta](../../README.md#beta)|
| Code Owners   |  [@open-telemetry/dotnet-contrib-maintainers](https://github.com/orgs/open-telemetry/teams/dotnet-contrib-maintainers)|

[![NuGet](https://img.shields.io/nuget/v/OpenTelemetry.Instrumentation.AseClient.svg)](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AseClient)
[![NuGet](https://img.shields.io/nuget/dt/OpenTelemetry.Instrumentation.AseClient.svg)](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AseClient)
[![codecov.io](https://codecov.io/gh/open-telemetry/opentelemetry-dotnet-contrib/branch/main/graphs/badge.svg?flag=unittests-Instrumentation.AseClient)](https://app.codecov.io/gh/open-telemetry/opentelemetry-dotnet-contrib?flags[0]=unittests-Instrumentation.AseClient)

This is an [Instrumentation
Library](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/glossary.md#instrumentation-library),
which instruments
[AdoNetCore.AseClient](https://github.com/DataAction/AdoNetCore.AseClient)
and collects traces about database operations.

## Steps to enable OpenTelemetry.Instrumentation.AseClient

### Step 1: Install Package

Add a reference to the
[`OpenTelemetry.Instrumentation.AseClient`](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AseClient)
package. Also, add any other instrumentations & exporters you will need.

```shell
dotnet add package --prerelease OpenTelemetry.Instrumentation.AseClient
```

### Step 2: Enable AseClient Instrumentation at application startup

AseClient instrumentation must be enabled at application startup.

The following example demonstrates adding AseClient instrumentation to a console
application. This example also sets up the OpenTelemetry Console exporter, which
requires adding the package
[`OpenTelemetry.Exporter.Console`](../OpenTelemetry.Exporter.Console/README.md)
to the application.

```csharp
using OpenTelemetry.Trace;

public class Program
{
    public static void Main(string[] args)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddAseClientInstrumentation()
            .AddConsoleExporter()
            .Build();
    }
}
```

For an ASP.NET Core application, adding instrumentation is typically done in the
`ConfigureServices` of your `Startup` class. Refer to documentation for
[OpenTelemetry.Instrumentation.AspNetCore](../OpenTelemetry.Instrumentation.AspNetCore/README.md).

For an ASP.NET application, adding instrumentation is typically done in the
`Global.asax.cs`. Refer to the documentation for
[OpenTelemetry.Instrumentation.AspNet](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNet/README.md).

## Advanced configuration

This instrumentation can be configured to change the default behavior by using
`AseClientTraceInstrumentationOptions`.

### SetDbStatementForText

Capturing the text of a database query may run the risk of capturing sensitive data.
`SetDbStatementForText` controls whether the
[`db.statement`](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#call-level-attributes)
attribute is captured in scenarios where there could be a risk of exposing
sensitive data. The behavior of `SetDbStatementForText` depends on the runtime
used.

#### .NET

On .NET, `SetDbStatementForText` controls whether or not
this instrumentation will set the
[`db.statement`](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#call-level-attributes)
attribute to the `CommandText` of the `AseCommand` being executed when the `CommandType`
is `CommandType.Text`. The
[`db.statement`](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#call-level-attributes)
attribute is always captured for `CommandType.StoredProcedure` because the `AseCommand.CommandText`
only contains the stored procedure name.

`SetDbStatementForText` is _false_ by default. When set to `true`, the
instrumentation will set
[`db.statement`](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#call-level-attributes)
attribute to the text of the SQL command being executed.

To enable capturing of `AseCommand.CommandText` for `CommandType.Text` use the
following configuration.

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAseClientInstrumentation(
        options => options.SetDbStatementForText = true)
    .AddConsoleExporter()
    .Build();
```

#### .NET Framework

On .NET Framework, there is no way to determine the type of command being
executed, so the `SetDbStatementForText` property always controls whether
or not this instrumentation will set the
[`db.statement`](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#call-level-attributes)
attribute to the `CommandText` of the `AseCommand` being executed. The
`CommandText` could be the name of a stored procedure (when
`CommandType.StoredProcedure` is used) or the full text of a `CommandType.Text`
query.

`SetDbStatementForText` is _false_ by default. When set to `true`, the
instrumentation will set
[`db.statement`](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#call-level-attributes)
attribute to the text of the SQL command being executed.

To enable capturing of `AseCommand.CommandText` for `CommandType.Text` use the
following configuration.

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAseClientInstrumentation(
        options => options.SetDbStatementForText = true)
    .AddConsoleExporter()
    .Build();
```

### EnableConnectionLevelAttributes

By default, `EnabledConnectionLevelAttributes` is enabled.
When `EnabledConnectionLevelAttributes` is enabled,
the [`DataSource`](https://docs.microsoft.com/dotnet/api/system.data.common.dbconnection.datasource)
will be parsed and the server name or IP address will be sent as
the `server.address` attribute, the instance name will be sent as
the `db.mssql.instance_name` attribute, and the port will be sent as the
`server.port` attribute if it is not 5000 (the default port).

The following example shows how to use `EnableConnectionLevelAttributes`.

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAseClientInstrumentation(
        options => options.EnableConnectionLevelAttributes = true)
    .AddConsoleExporter()
    .Build();
```

### Enrich

This option can be used to enrich the activity with additional information from
the raw `AseCommand` object. The `Enrich` action is called only when
`activity.IsAllDataRequested` is `true`. It contains the activity itself (which
can be enriched), the name of the event, and the actual raw object.

Currently there is only one event name reported, "OnCustom". The actual object
is `AdoNetCore.AseClient.AseCommand`.

The following code snippet shows how to add additional tags using `Enrich`.

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAseClientInstrumentation(opt => opt.Enrich
        = (activity, eventName, rawObject) =>
    {
        if (eventName.Equals("OnCustom"))
        {
            if (rawObject is AseCommand cmd)
            {
                activity.SetTag("db.commandTimeout", cmd.CommandTimeout);
            }
        };
    })
    .Build();
```

[Processor](../../docs/trace/extending-the-sdk/README.md#processor), is the
general extensibility point to add additional properties to any activity. The
`Enrich` option is specific to this instrumentation, and is provided to get
access to `AseCommand` object.

### RecordException

This option can be set to instruct the instrumentation to record AseExceptions
as Activity
[events](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/exceptions/exceptions-spans.md).

The default value is `false` and can be changed by the code like below.

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAseClientInstrumentation(
        options => options.RecordException = true)
    .AddConsoleExporter()
    .Build();
```

### Filter

This option can be used to filter out activities based on the properties of the
`AseCommand` object being instrumented using a `Func<object, bool>`. The
function receives an instance of the raw `AseCommand` and should return `true`
if the telemetry is to be collected, and `false` if it should not. The parameter
of the Func delegate is of type `object` and needs to be cast to the appropriate
type of `AseCommand`. The example below filters out all commands
that are not stored procedures.

```csharp
using var traceProvider = Sdk.CreateTracerProviderBuilder()
   .AddAseClientInstrumentation(
       opt =>
       {
           opt.Filter = cmd =>
           {
               if (cmd is AseCommand command)
               {
                   return command.CommandType == CommandType.StoredProcedure;
               }

               return false;
           };
       })
   .AddConsoleExporter()
   .Build();
{
```

## References

* [OpenTelemetry Project](https://opentelemetry.io/)

* [OpenTelemetry semantic conventions for database
  calls](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md)
