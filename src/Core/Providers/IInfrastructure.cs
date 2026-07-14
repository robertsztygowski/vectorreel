namespace VectorReel.Core.Providers;

/// <summary>
/// Stage C — fusion of the ordered segment analyses into one document (text-only model call).
///
/// 🚧 <b>Not implemented in Phase 1.</b> Phase 2 owns it.
/// </summary>
public interface ITextFuser
{
    /// <summary>Merge overlap duplicates, find topic boundaries, and produce the final document.</summary>
    Task<string> FuseAsync(IReadOnlyList<SegmentAnalysis> segments, CancellationToken cancellationToken);
}

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
