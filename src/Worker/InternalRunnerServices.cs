using MdReel.Core.Domain;
using MdReel.Core.Providers;

namespace MdReel.Worker;

internal sealed class LocalFileObjectStorage(IHostEnvironment environment) : IObjectStorage
{
    private readonly string _root = Path.Combine(environment.ContentRootPath, ".local-state", "internal-object-storage");

    public async Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken)
    {
        var path = ResolvePath(bucket, objectName);
        var stream = File.OpenRead(path);
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
        return stream;
    }

    public async Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken)
    {
        var path = ResolvePath(bucket, objectName);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
        await file.FlushAsync(cancellationToken);
    }

    public Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken)
    {
        var path = ResolvePath(bucket, objectName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    private string ResolvePath(string bucket, string objectName)
    {
        var sanitizedObject = objectName.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_root, bucket, sanitizedObject);
    }
}

internal sealed class UnconfiguredVideoAnalyzer : IVideoAnalyzer
{
    public Task<StageBModelResponse> AnalyzeAsync(
        string sourceUri,
        Segment segment,
        StageBCallOptions callOptions,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            "No IVideoAnalyzer implementation is configured. Wire a Vertex-backed analyzer before enabling YouTubeGalleryRunner.");
    }
}

internal sealed class UnconfiguredTextFuser : ITextFuser
{
    public Task<string> FuseAsync(IReadOnlyList<SegmentAnalysis> segments, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            "No ITextFuser implementation is configured. Wire a Stage-C text fuser before enabling YouTubeGalleryRunner.");
    }
}
