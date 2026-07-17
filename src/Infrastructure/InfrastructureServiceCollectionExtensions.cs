using MdReel.Core.Providers;
using MdReel.Infrastructure.Vertex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MdReel.Infrastructure;

/// <summary>Wires the Vertex-backed Stage B/C providers behind the Core seams.</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="VertexVideoAnalyzer"/> (<see cref="IVideoAnalyzer"/>) and
    /// <see cref="VertexTextFuser"/> (<see cref="ITextFuser"/>) with typed HttpClients and ADC auth.
    /// EU-region defaults come from <see cref="VertexOptions"/>; override via the "Vertex" config
    /// section.
    /// </summary>
    public static IServiceCollection AddVertexInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<VertexOptions>()
            .Bind(configuration.GetSection(VertexOptions.SectionName));

        services.AddSingleton<IAccessTokenProvider, AdcAccessTokenProvider>();

        // Video (Stage B) calls hold a connection open for minutes; the per-call wall-clock cap is
        // the runner's linked timeout (rule 9), so the HttpClient timeout is a generous backstop.
        services.AddHttpClient<IVideoAnalyzer, VertexVideoAnalyzer>(client =>
            client.Timeout = TimeSpan.FromMinutes(10));

        services.AddHttpClient<ITextFuser, VertexTextFuser>(client =>
            client.Timeout = TimeSpan.FromMinutes(5));

        return services;
    }
}
