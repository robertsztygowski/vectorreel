using Google;
using Google.Apis.Http;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdReel.Infrastructure.Storage;

/// <summary>
/// Real GCS <see cref="MdReel.Core.Providers.IObjectStorage"/>. ADC in production, the
/// fake-gcs-server emulator locally when <see cref="GcsOptions.EmulatorHost"/> is set. No JSON key
/// files (CLAUDE.md rule 1).
/// </summary>
public sealed class GcsObjectStorage : MdReel.Core.Providers.IObjectStorage, IDisposable
{
    private readonly StorageClient _client;
    private readonly GcsOptions _options;
    private readonly ILogger<GcsObjectStorage> _logger;
    private readonly bool _isEmulator;

    public GcsObjectStorage(IOptions<GcsOptions> options, ILogger<GcsObjectStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _isEmulator = !string.IsNullOrWhiteSpace(_options.EmulatorHost);

        _client = _isEmulator ? CreateEmulatorClient(_options.EmulatorHost!) : StorageClient.Create();
    }

    public async Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"mdreel-gcs-{Guid.NewGuid():N}.tmp");
        var file = new FileStream(
            tempPath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            await _client.DownloadObjectAsync(bucket, objectName, file, cancellationToken: cancellationToken);
            file.Position = 0;
            return file;
        }
        catch
        {
            await file.DisposeAsync();
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }

    public async Task DownloadToFileAsync(
        string bucket,
        string objectName,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var file = File.Create(destinationPath);
        await _client.DownloadObjectAsync(bucket, objectName, file, cancellationToken: cancellationToken);
    }

    public async Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken)
    {
        if (_isEmulator)
        {
            await EnsureBucketAsync(bucket, cancellationToken);
        }

        await _client.UploadObjectAsync(bucket, objectName, contentType: null, content, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken)
    {
        try
        {
            await _client.DeleteObjectAsync(bucket, objectName, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Delete of gs://{Bucket}/{Object} was a no-op (already gone)", bucket, objectName);
            }
        }
    }

    private async Task EnsureBucketAsync(string bucket, CancellationToken cancellationToken)
    {
        try
        {
            await _client.GetBucketAsync(bucket, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _client.CreateBucketAsync(_options.Project, bucket, cancellationToken: cancellationToken);
        }
    }

    private static StorageClient CreateEmulatorClient(string emulatorHost)
    {
        var baseUri = emulatorHost.TrimEnd('/');
        if (!baseUri.EndsWith("/storage/v1/", StringComparison.Ordinal))
        {
            baseUri += "/storage/v1/";
        }

        return new StorageClientBuilder
        {
            BaseUri = baseUri,
            UnauthenticatedAccess = true,
            HttpClientFactory = new EmulatorHttpClientFactory(),
        }.Build();
    }

    public void Dispose() => _client.Dispose();

    // fake-gcs-server rewrites download URLs to its external-url; the default factory is fine, but a
    // named factory keeps the emulator client from sharing the ADC client's message handlers.
    private sealed class EmulatorHttpClientFactory : HttpClientFactory
    {
    }
}
