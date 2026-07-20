using MdReel.Infrastructure.Telemetry;
using Microsoft.Extensions.Configuration;

namespace MdReel.Tests.Unit.Telemetry;

public sealed class TelemetryExportModeTests
{
    [Fact]
    public void ResolveExportMode_ReturnsNone_WhenNoExporterIsConfigured()
    {
        var configuration = BuildConfiguration([]);

        Assert.Equal(MdreelTelemetryExportMode.None, TelemetryServiceCollectionExtensions.ResolveExportMode(configuration));
    }

    [Fact]
    public void ResolveExportMode_ReturnsOtlp_WhenOnlyOtlpEndpointIsConfigured()
    {
        var configuration = BuildConfiguration(
        [
            new KeyValuePair<string, string?>("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317"),
        ]);

        Assert.Equal(MdreelTelemetryExportMode.Otlp, TelemetryServiceCollectionExtensions.ResolveExportMode(configuration));
    }

    [Fact]
    public void ResolveExportMode_ReturnsGoogleCloud_WhenProjectIdIsConfigured()
    {
        var configuration = BuildConfiguration(
        [
            new KeyValuePair<string, string?>("Telemetry:GcpProjectId", "test-project"),
        ]);

        Assert.Equal(MdreelTelemetryExportMode.GoogleCloud, TelemetryServiceCollectionExtensions.ResolveExportMode(configuration));
    }

    [Fact]
    public void ResolveExportMode_ReturnsGoogleCloud_WhenBothProjectIdAndOtlpEndpointAreConfigured()
    {
        var configuration = BuildConfiguration(
        [
            new KeyValuePair<string, string?>("Telemetry:GcpProjectId", "test-project"),
            new KeyValuePair<string, string?>("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317"),
        ]);

        Assert.Equal(MdreelTelemetryExportMode.GoogleCloud, TelemetryServiceCollectionExtensions.ResolveExportMode(configuration));
    }

    [Fact]
    public void GetGoogleCloudProjectId_SupportsLegacyFlatEnvironmentAlias()
    {
        var configuration = BuildConfiguration(
        [
            new KeyValuePair<string, string?>("TELEMETRY_GCP_PROJECT_ID", "test-project"),
        ]);

        Assert.Equal("test-project", TelemetryServiceCollectionExtensions.GetGoogleCloudProjectId(configuration));
    }

    private static IConfiguration BuildConfiguration(IEnumerable<KeyValuePair<string, string?>> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
