using System.Net.Http.Json;
using System.Text;
using MdReel.Core.Providers;
using MdReel.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MdReel.Tests.Integration;

public sealed class ObjectStorageDownloadTests
{
    [Fact]
    public async Task DownloadToFileAsync_default_implementation_round_trips_local_storage()
    {
        var root = Path.Combine(
            Fixtures.RepoRoot,
            ".local-state",
            "tests",
            $"local-object-storage-{Guid.NewGuid():N}");
        var destination = Path.Combine(root, "downloaded.txt");
        try
        {
            IObjectStorage storage = new LocalDirectoryObjectStorage(root);
            await using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello local storage"));
            await storage.WriteAsync("bucket", "path/object.txt", content, CancellationToken.None);

            await storage.DownloadToFileAsync("bucket", "path/object.txt", destination, CancellationToken.None);

            Assert.Equal("hello local storage", await File.ReadAllTextAsync(destination));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    [Trait("Category", "RequiresDocker")]
    public async Task GcsObjectStorage_DownloadToFileAsync_round_trips_against_emulator()
    {
        using var storage = new GcsObjectStorage(
            Options.Create(new GcsOptions
            {
                EmulatorHost = "http://localhost:4443",
                Project = "vectorreel-tests",
            }),
            NullLogger<GcsObjectStorage>.Instance);
        var bucket = $"download-test-{Guid.NewGuid():N}";
        var destinationRoot = Path.Combine(Fixtures.RepoRoot, ".local-state", "tests", bucket);
        var destination = Path.Combine(destinationRoot, "downloaded.txt");
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri("http://localhost:4443") };
            using var createBucket = await http.PostAsJsonAsync(
                "/storage/v1/b?project=vectorreel-tests",
                new { name = bucket },
                CancellationToken.None);
            createBucket.EnsureSuccessStatusCode();

            using var content = new ByteArrayContent(Encoding.UTF8.GetBytes("hello fake gcs"));
            using var upload = await http.PostAsync(
                $"/upload/storage/v1/b/{bucket}/o?uploadType=media&name={Uri.EscapeDataString("uploads/object.txt")}",
                content,
                CancellationToken.None);
            upload.EnsureSuccessStatusCode();

            await storage.DownloadToFileAsync(bucket, "uploads/object.txt", destination, CancellationToken.None);

            Assert.Equal("hello fake gcs", await File.ReadAllTextAsync(destination));
        }
        finally
        {
            if (Directory.Exists(destinationRoot))
            {
                Directory.Delete(destinationRoot, recursive: true);
            }
        }
    }
}
