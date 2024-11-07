// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using OpenTelemetry.Instrumentation.AseClient.Implementation;
using OpenTelemetry.Tests;

using Xunit;

namespace OpenTelemetry.Instrumentation.AseClient.Tests;

public class EventSourceTest
{
    [Fact]
    public void EventSourceTest_AseClientInstrumentationEventSource()
    {
        EventSourceTestHelper.MethodsAreImplementedConsistentlyWithTheirAttributes(AseClientInstrumentationEventSource.Log);
    }
}
