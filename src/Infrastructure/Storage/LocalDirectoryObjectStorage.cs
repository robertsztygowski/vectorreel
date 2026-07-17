using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Storage;

/// <summary>
/// Local-filesystem <see cref="IObjectStorage"/> for local dev and the e2e compose profile, where
/// Vertex never reads the bytes (the pipeline runs in replay/fake mode). Buckets map to directories
/// under <see cref="RootPath"/>.
/// </summary>
public sealed class LocalDirectoryObjectStorage(string rootPath) : IObjectStorage
{
    public string RootPath { get; } = rootPath;

    public async Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(bucket, objectName);
        Stream stream = File.OpenRead(path);
        await Task.CompletedTask;
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
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(bucket, objectName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string bucket, string objectName)
    {
        var sanitizedObject = objectName.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(RootPath, bucket, sanitizedObject);
    }
}
