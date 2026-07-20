using MdReel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MdReel.Infrastructure.Telemetry;

public static class TelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddMdreelOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        if (string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            return services;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddHttpClientInstrumentation()
                    .AddSource(PipelineDiagnostics.SourceName)
                    .AddOtlpExporter();
                configureTracing?.Invoke(tracing);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddMeter(PipelineDiagnostics.SourceName)
                    .AddOtlpExporter();
                configureMetrics?.Invoke(metrics);
            })
            .WithLogging(logging => logging.AddOtlpExporter());

        return services;
    }
}
