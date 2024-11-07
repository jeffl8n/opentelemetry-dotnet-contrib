// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using OpenTelemetry.Instrumentation.AseClient.Implementation;
using Xunit;

namespace OpenTelemetry.Instrumentation.AseClient.Tests;

public class AseConnectionDetailsTests
{
    [Theory]
    [InlineData("localhost", "localhost", null, null)]
    [InlineData("127.0.0.1", null, "127.0.0.1", null)]
    [InlineData("127.0.0.1,5000", null, "127.0.0.1", null)]
    [InlineData("127.0.0.1, 2500", null, "127.0.0.1", 2500)]
    [InlineData("tcp:localhost", "localhost", null, null)]
    [InlineData("tcp : localhost", "localhost", null, null)]
    [InlineData("np : localhost", "localhost", null, null)]
    [InlineData("lpc:localhost", "localhost", null, null)]
    [InlineData("np:\\\\localhost\\pipe\\sybase\\query", "localhost", null, null)]
    [InlineData("np : \\\\localhost\\pipe\\sybase\\query", "localhost", null, null)]
    public void ParseFromDataSourceTests(
        string dataSource,
        string? expectedServerHostName,
        string? expectedServerIpAddress,
        int? expectedPort)
    {
        var sqlConnectionDetails = AseConnectionDetails.ParseFromDataSource(dataSource);

        Assert.NotNull(sqlConnectionDetails);
        Assert.Equal(expectedServerHostName, sqlConnectionDetails.ServerHostName);
        Assert.Equal(expectedServerIpAddress, sqlConnectionDetails.ServerIpAddress);
        Assert.Equal(expectedPort, sqlConnectionDetails.Port);
    }
}
