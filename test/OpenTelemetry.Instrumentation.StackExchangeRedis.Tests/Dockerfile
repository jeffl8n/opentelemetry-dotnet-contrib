# Create a container for running the OpenTelemetry Redis integration tests.
# This should be run from the root of the repo:
# docker build --file test/OpenTelemetry.Instrumentation.StackExchangeRedis.Tests/Dockerfile .

ARG BUILD_SDK_VERSION=9.0
ARG TEST_SDK_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${BUILD_SDK_VERSION} AS build
ARG PUBLISH_CONFIGURATION=Release
ARG PUBLISH_FRAMEWORK=net9.0
WORKDIR /repo
COPY . ./
WORKDIR "/repo/test/OpenTelemetry.Instrumentation.StackExchangeRedis.Tests"
RUN dotnet publish "OpenTelemetry.Instrumentation.StackExchangeRedis.Tests.csproj" -c "${PUBLISH_CONFIGURATION}" -f "${PUBLISH_FRAMEWORK}" -o /drop -p:IntegrationBuild=true

FROM mcr.microsoft.com/dotnet/sdk:${TEST_SDK_VERSION} AS final
WORKDIR /test
COPY --from=build /drop .
ENTRYPOINT ["dotnet", "vstest", "OpenTelemetry.Instrumentation.StackExchangeRedis.Tests.dll"]
