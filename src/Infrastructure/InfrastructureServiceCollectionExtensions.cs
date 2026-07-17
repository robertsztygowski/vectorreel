using MdReel.Core.Providers;
using MdReel.Infrastructure.Pipeline;
using MdReel.Infrastructure.Storage;
using MdReel.Infrastructure.Vertex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MdReel.Infrastructure;

/// <summary>
/// Wires the concrete providers behind the Core seams: object storage (GCS or local) and the
/// Stage B / Stage C model (fake / live / replay / record — <see cref="PipelineModelMode"/>).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IObjectStorage"/> and the Stage B/C model seams from configuration.
    /// Safe defaults: local-directory storage and the <see cref="PipelineModelMode.Fake"/> model, so
    /// nothing touches the cloud or spends until explicitly configured.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">Application configuration (Vertex / Gcs / PipelineModel sections).</param>
    /// <param name="defaultLocalStorageRoot">
    /// Where <see cref="LocalDirectoryObjectStorage"/> keeps buckets when GCS is not configured.
    /// </param>
    public static IServiceCollection AddPipelineInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string defaultLocalStorageRoot)
    {
        services.AddOptions<VertexOptions>().Bind(configuration.GetSection(VertexOptions.SectionName));
        services.AddOptions<GcsOptions>().Bind(configuration.GetSection(GcsOptions.SectionName));
        services.AddOptions<PipelineModelOptions>().Bind(configuration.GetSection(PipelineModelOptions.SectionName));

        AddObjectStorage(services, configuration, defaultLocalStorageRoot);
        AddModel(services, configuration);

        return services;
    }

    private static void AddObjectStorage(
        IServiceCollection services,
        IConfiguration configuration,
        string defaultLocalStorageRoot)
    {
        var useGcs = !string.IsNullOrWhiteSpace(configuration[$"{GcsOptions.SectionName}:EmulatorHost"])
            || string.Equals(configuration["Storage:Mode"], "gcs", StringComparison.OrdinalIgnoreCase);

        if (useGcs)
        {
            services.AddSingleton<IObjectStorage, GcsObjectStorage>();
            return;
        }

        var root = configuration["Storage:LocalRoot"] ?? defaultLocalStorageRoot;
        services.AddSingleton<IObjectStorage>(new LocalDirectoryObjectStorage(root));
    }

    private static void AddModel(IServiceCollection services, IConfiguration configuration)
    {
        var mode = ParseMode(configuration[$"{PipelineModelOptions.SectionName}:Mode"]);

        if (mode is PipelineModelMode.Live or PipelineModelMode.Record)
        {
            RegisterVertex(services);
        }

        if (mode is PipelineModelMode.Replay or PipelineModelMode.Record)
        {
            services.AddSingleton(sp =>
            {
                var dir = sp.GetRequiredService<IOptions<PipelineModelOptions>>().Value.FixturesDirectory
                    ?? throw new InvalidOperationException(
                        "PipelineModel:FixturesDirectory is required for Replay/Record modes.");
                return new FixtureStore(dir);
            });
        }

        switch (mode)
        {
            case PipelineModelMode.Fake:
                services.AddSingleton<IVideoAnalyzer, FakeVideoAnalyzer>();
                services.AddSingleton<ITextFuser, FakeTextFuser>();
                break;

            case PipelineModelMode.Live:
                services.AddSingleton<IVideoAnalyzer>(sp => sp.GetRequiredService<VertexVideoAnalyzer>());
                services.AddSingleton<ITextFuser>(sp => sp.GetRequiredService<VertexTextFuser>());
                break;

            case PipelineModelMode.Replay:
                services.AddSingleton<IVideoAnalyzer, ReplayVideoAnalyzer>();
                services.AddSingleton<ITextFuser, ReplayTextFuser>();
                break;

            case PipelineModelMode.Record:
                services.AddSingleton<IVideoAnalyzer>(sp => new RecordingVideoAnalyzer(
                    sp.GetRequiredService<VertexVideoAnalyzer>(), sp.GetRequiredService<FixtureStore>()));
                services.AddSingleton<ITextFuser>(sp => new RecordingTextFuser(
                    sp.GetRequiredService<VertexTextFuser>(), sp.GetRequiredService<FixtureStore>()));
                break;

            default:
                throw new InvalidOperationException($"Unhandled pipeline model mode '{mode}'.");
        }
    }

    private static void RegisterVertex(IServiceCollection services)
    {
        services.TryAddSingleton<IAccessTokenProvider, AdcAccessTokenProvider>();

        // Video (Stage B) calls hold a connection open for minutes; the per-call wall-clock cap is
        // the runner's linked timeout (rule 9), so the HttpClient timeout is a generous backstop.
        services.AddHttpClient<VertexVideoAnalyzer>(client => client.Timeout = TimeSpan.FromMinutes(10));
        services.AddHttpClient<VertexTextFuser>(client => client.Timeout = TimeSpan.FromMinutes(5));
    }

    private static PipelineModelMode ParseMode(string? raw) =>
        raw?.Trim().ToLowerInvariant() switch
        {
            null or "" or "fake" => PipelineModelMode.Fake,
            "live" => PipelineModelMode.Live,
            "replay" => PipelineModelMode.Replay,
            "record" => PipelineModelMode.Record,
            _ => throw new InvalidOperationException(
                $"Unknown PipelineModel:Mode '{raw}'. Expected fake, live, replay, or record."),
        };
}
