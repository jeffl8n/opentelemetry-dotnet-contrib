# Changelog

## Unreleased

* Initial release of `OpenTelemetry.Instrumentation.AseClient`.
* Added support for tracing database operations using `AdoNetCore.AseClient`.
* Implemented `AseClientInstrumentation` class for instrumentation.
* Added `AseClientTraceInstrumentationOptions` for configuration.
* Implemented `TracerProviderBuilderExtensions` to enable `AseClient` instrumentation.
* Added `AseActivitySourceHelper` for common properties and methods.
* Implemented `AseClientDiagnosticListener` to handle `AseClient`-specific events.
* Added `AseClientInstrumentationEventSource` for logging and error handling.
* Implemented `AseConnectionDetails` to parse `AseClient` connection strings.
* Added tests for `AseClient` instrumentation.
