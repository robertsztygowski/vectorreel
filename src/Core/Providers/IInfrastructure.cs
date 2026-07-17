using MdReel.Core.Output;

namespace MdReel.Core.Providers;

/// <summary>
/// Stage C — fusion of the ordered segment analyses into one document (text-only model call).
/// Produces the structured <see cref="OutputDocument"/>; Stage D renders it to Markdown + JSON and
/// persists it (ARCHITECTURE.md §3–§4).
/// </summary>
public interface ITextFuser
{
    /// <summary>Merge overlap duplicates, find topic boundaries, and produce the final document.</summary>
    Task<OutputDocument> FuseAsync(
        IReadOnlyList<SegmentAnalysis> segments,
        FusionRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// The pipeline-supplied facts Stage C must not invent — the model decides title/summary/tags/
/// sections; the pipeline owns provenance and identity.
/// </summary>
/// <param name="Source">Upload filename (private path) or canonical source URL (internal ingest).</param>
/// <param name="Duration">Source duration, from Stage A / the ingest window.</param>
/// <param name="Provenance">
/// Markdown body of the final "## Source &amp; licence" section: attribution + licence for gallery
/// outputs, the source-deletion/retention statement for private uploads.
/// </param>
/// <param name="Generator">mdreel@&lt;version&gt; string stamped into the frontmatter.</param>
/// <param name="JobId">
/// Owning job id, so Stage C's Vertex call can be recorded against it in the cost ledger
/// (CLAUDE.md rule 6). Empty for tests / fakes that do not meter.
/// </param>
public sealed record FusionRequest(
    string Source,
    TimeSpan Duration,
    string Provenance,
    string Generator,
    string JobId = "");

/// <summary>
/// Object storage. GCS today (<c>raw-videos-eu</c>, <c>outputs-eu</c>); the interface exists because
/// EU-owned infrastructure and a self-hosted edition are on the roadmap (ARCHITECTURE.md §7).
///
/// 🚧 <b>Not implemented in Phase 1.</b> Stage A reads a local file and writes nothing.
/// </summary>
public interface IObjectStorage
{
    /// <summary>Read an object.</summary>
    Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken);

    /// <summary>Write an object.</summary>
    Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken);

    /// <summary>Delete an object. Source videos are deleted after processing by default (ARCHITECTURE.md §3, Stage D).</summary>
    Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken);
}

/// <summary>
/// Work queue. <c>CloudTasksQueue</c> when deployed, <c>InProcessQueue</c> locally and in tests
/// (DEVELOPMENT.md §5 — Cloud Tasks has no emulator).
///
/// 🚧 <b>Not implemented in Phase 1.</b>
/// </summary>
public interface ITaskQueue
{
    /// <summary>Enqueue work for a stage.</summary>
    Task EnqueueAsync(string queue, string payload, CancellationToken cancellationToken);
}
