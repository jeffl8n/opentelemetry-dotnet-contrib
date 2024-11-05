using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AdoNetCore.AseClient;
using OpenTelemetry.Trace;
using Xunit;

namespace OpenTelemetry.Instrumentation.AseClient.Tests
{
    public class AseClientInstrumentationTests : IDisposable
    {
        private readonly TracerProvider tracerProvider;

        public AseClientInstrumentationTests()
        {
            this.tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddAseClientInstrumentation()
                .Build();
        }

        [Fact]
        public async Task AseClientInstrumentation_BasicTest()
        {
            using var connection = new AseConnection("Data Source=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Assert.Equal(1, reader.GetInt32(0));
            }
        }

        [Fact]
        public void AseClientInstrumentation_ConfigurationOptionsTest()
        {
            var options = new AseClientTraceInstrumentationOptions
            {
                SetDbStatementForText = true,
                EnableConnectionLevelAttributes = true,
                RecordException = true,
                Enrich = (activity, eventName, rawObject) =>
                {
                    if (rawObject is AseCommand command)
                    {
                        activity.SetTag("db.command", command.CommandText);
                    }
                },
                Filter = rawObject => rawObject is AseCommand command && command.CommandText != "SELECT 1"
            };

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddAseClientInstrumentation(o => o = options)
                .Build();

            using var connection = new AseConnection("Data Source=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Assert.Equal(1, reader.GetInt32(0));
            }
        }

        public void Dispose()
        {
            this.tracerProvider?.Dispose();
        }
    }
}
