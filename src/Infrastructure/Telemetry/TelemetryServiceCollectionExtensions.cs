using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Logging.Console;
using MdReel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MdReel.Infrastructure.Telemetry;

public enum MdreelTelemetryExportMode
{
    None,
    Otlp,
    GoogleCloud,
}

public static class TelemetryServiceCollectionExtensions
{
    private const string GoogleCloudProjectIdConfigurationKey = "Telemetry:GcpProjectId";
    private const string GoogleCloudProjectIdEnvironmentKey = "TELEMETRY_GCP_PROJECT_ID";
    private const string GoogleCloudOtlpEndpoint = "https://telemetry.googleapis.com";
    private const string CloudPlatformScope = "https://www.googleapis.com/auth/cloud-platform";

    public static IServiceCollection AddMdreelOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        var exportMode = ResolveExportMode(configuration);
        if (exportMode == MdreelTelemetryExportMode.None)
        {
            return services;
        }

        var googleCloudProjectId = GetGoogleCloudProjectId(configuration);
        services.AddOpenTelemetry()
            .ConfigureResource(resource => ConfigureResource(resource, serviceName, googleCloudProjectId))
            .WithTracing(tracing =>
            {
                tracing
                    .AddHttpClientInstrumentation()
                    .AddSource(PipelineDiagnostics.SourceName);
                configureTracing?.Invoke(tracing);
                AddTraceExporter(tracing, exportMode, googleCloudProjectId);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddMeter(PipelineDiagnostics.SourceName);
                configureMetrics?.Invoke(metrics);
                AddMetricExporter(metrics, exportMode, googleCloudProjectId);
            })
            .WithLogging(logging =>
            {
                if (exportMode == MdreelTelemetryExportMode.Otlp)
                {
                    logging.AddOtlpExporter();
                }
            });

        return services;
    }

    public static ILoggingBuilder AddMdreelGoogleCloudConsole(
        this ILoggingBuilder logging,
        IConfiguration configuration)
    {
        var googleCloudProjectId = GetGoogleCloudProjectId(configuration);
        if (string.IsNullOrWhiteSpace(googleCloudProjectId))
        {
            return logging;
        }

        logging.AddGoogleCloudConsole(options => options.TraceGoogleCloudProjectId = googleCloudProjectId);
        return logging;
    }

    public static MdreelTelemetryExportMode ResolveExportMode(IConfiguration configuration)
    {
        // Production GCP export deliberately wins if both switches are present: Cloud Run should send
        // traces/metrics to Google Cloud and logs to structured stdout; local compose keeps Aspire OTLP.
        if (!string.IsNullOrWhiteSpace(GetGoogleCloudProjectId(configuration)))
        {
            return MdreelTelemetryExportMode.GoogleCloud;
        }

        return string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
            ? MdreelTelemetryExportMode.None
            : MdreelTelemetryExportMode.Otlp;
    }

    public static string? GetGoogleCloudProjectId(IConfiguration configuration) =>
        configuration[GoogleCloudProjectIdConfigurationKey] ?? configuration[GoogleCloudProjectIdEnvironmentKey];

    private static void ConfigureResource(ResourceBuilder resource, string serviceName, string? googleCloudProjectId)
    {
        resource.AddService(serviceName);

        if (!string.IsNullOrWhiteSpace(googleCloudProjectId))
        {
            // telemetry.googleapis.com OTLP ingest rejects payloads without this attribute
            // ("Resource is missing required attribute \"gcp.project_id\"", HTTP 400), and the
            // metrics signal additionally rejects resources without a resolvable location
            // ("write for resource failed: Unrecognized region or location.").
            var region = Environment.GetEnvironmentVariable("TELEMETRY_CLOUD_REGION");
            resource.AddAttributes(
            [
                new KeyValuePair<string, object>("gcp.project_id", googleCloudProjectId),
                new KeyValuePair<string, object>("cloud.region", string.IsNullOrWhiteSpace(region) ? "europe-west1" : region),
            ]);
        }

        var revision = Environment.GetEnvironmentVariable("K_REVISION");
        if (!string.IsNullOrWhiteSpace(revision))
        {
            resource.AddAttributes(
            [
                new KeyValuePair<string, object>("service.version", revision),
                new KeyValuePair<string, object>("gcp.cloud_run.revision", revision),
            ]);
        }
    }

    private static void AddTraceExporter(
        TracerProviderBuilder tracing,
        MdreelTelemetryExportMode exportMode,
        string? googleCloudProjectId)
    {
        if (exportMode == MdreelTelemetryExportMode.GoogleCloud)
        {
            // NuGet has no GA Google Cloud OTel trace exporter for .NET today; use the GA Google
            // OTLP ingest endpoint with ADC/runtime-service-account OAuth instead.
            // Cloud Run's front-end injects an UNSAMPLED traceparent into every request; the default
            // ParentBased sampler would inherit that and drop every server span, so sample explicitly.
            // MVP volume sits well inside the Cloud Trace free tier; revisit with a ratio sampler if
            // traffic ever makes tracing a line item.
            tracing.SetSampler(new AlwaysOnSampler());
            tracing.AddOtlpExporter(options => ConfigureGoogleCloudOtlp(options, googleCloudProjectId, "traces"));
            return;
        }

        tracing.AddOtlpExporter();
    }

    private static void AddMetricExporter(
        MeterProviderBuilder metrics,
        MdreelTelemetryExportMode exportMode,
        string? googleCloudProjectId)
    {
        if (exportMode == MdreelTelemetryExportMode.GoogleCloud)
        {
            // NuGet has no GA Google Cloud OTel metrics exporter for .NET today; use the GA Google
            // OTLP ingest endpoint with ADC/runtime-service-account OAuth instead.
            metrics.AddOtlpExporter(options => ConfigureGoogleCloudOtlp(options, googleCloudProjectId, "metrics"));
            return;
        }

        metrics.AddOtlpExporter();
    }

    private static void ConfigureGoogleCloudOtlp(
        OtlpExporterOptions options,
        string? googleCloudProjectId,
        string signalPath)
    {
        if (string.IsNullOrWhiteSpace(googleCloudProjectId))
        {
            throw new InvalidOperationException("Google Cloud telemetry export requires Telemetry:GcpProjectId.");
        }

        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Endpoint = new Uri($"{GoogleCloudOtlpEndpoint}/v1/{signalPath}");
        options.HttpClientFactory = () => new HttpClient(new GoogleAuthenticatedHttpMessageHandler(googleCloudProjectId));
    }

    private sealed class GoogleAuthenticatedHttpMessageHandler(string projectId) : DelegatingHandler(new HttpClientHandler())
    {
        private readonly Lazy<Task<ITokenAccess>> credential = new(CreateCredentialAsync);

        // The OTLP HTTP exporter uses the SYNCHRONOUS HttpClient.Send path, which dispatches to
        // Send below — an async-only override silently never runs and every export gets 403.
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorize(request, cancellationToken);
            return base.Send(request, cancellationToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Authorize(request, cancellationToken);
            return base.SendAsync(request, cancellationToken);
        }

        private void Authorize(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = credential.Value
                .ConfigureAwait(false).GetAwaiter().GetResult()
                .GetAccessTokenForRequestAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.TryAddWithoutValidation("x-goog-user-project", projectId);
        }

        private static async Task<ITokenAccess> CreateCredentialAsync()
        {
            var googleCredential = await GoogleCredential.GetApplicationDefaultAsync().ConfigureAwait(false);
            googleCredential = googleCredential.IsCreateScopedRequired
                ? googleCredential.CreateScoped(CloudPlatformScope)
                : googleCredential;
            return (ITokenAccess)googleCredential.UnderlyingCredential;
        }
    }
}
