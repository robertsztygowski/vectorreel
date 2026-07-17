using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MdReel.Core.Providers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MdReel.Tests.Integration;

/// <summary>
/// Item 2 — the private path stages the uploaded raw bytes into <c>raw-videos-eu</c> and hands
/// Stage B a <c>gs://</c> URI, then erases the object after Stage D (ARCHITECTURE §7 tenant-isolation
/// + auto-deletion). Runs offline: fake model mode with staging forced on and a spy
/// <see cref="IObjectStorage"/> that records the write then the erase.
/// </summary>
[Trait("Category", "RequiresDocker")]
public sealed class PrivatePathGcsStagingTests
{
    [Fact]
    public async Task Raw_upload_is_staged_to_raw_videos_eu_then_erased()
    {
        var storage = new SpyObjectStorage();
        await using var factory = new StagingApiFactory(storage);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

        var (uploadId, uploadUrl) = await CreateUploadAsync(client);
        await PutBytesAsync(client, uploadUrl, Fixtures.TalkingHead);
        var jobId = await CreateJobAsync(client, uploadId, "talking_head.mp4");

        await WaitForDoneAsync(client, jobId);

        var expectedObject = $"private/{jobId}/source.mp4";

        var write = Assert.Single(
            storage.Writes,
            op => op.Bucket == "raw-videos-eu" && op.ObjectName == expectedObject);
        Assert.True(write.ByteCount > 0, "the staged raw object must contain the uploaded bytes");

        // The source is auto-deleted after Stage D (ARCHITECTURE §3/§7): written, then erased.
        Assert.Contains(
            storage.Deletes,
            op => op.Bucket == "raw-videos-eu" && op.ObjectName == expectedObject);
    }

    private static async Task<(string UploadId, string UploadUrl)> CreateUploadAsync(HttpClient client)
    {
        using var response = await client.PostAsync("/api/v1/uploads", content: null);
        response.EnsureSuccessStatusCode();
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        var uploadId = payload.RootElement.GetProperty("uploadId").GetString();
        var uploadUrl = payload.RootElement.GetProperty("uploadUrl").GetString();
        Assert.False(string.IsNullOrWhiteSpace(uploadId));
        Assert.False(string.IsNullOrWhiteSpace(uploadUrl));
        return (uploadId!, uploadUrl!);
    }

    private static async Task PutBytesAsync(HttpClient client, string uploadUrl, string filePath)
    {
        var bytes = await File.ReadAllBytesAsync(filePath);
        using var content = new ByteArrayContent(bytes);
        using var response = await client.PutAsync(uploadUrl, content);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task<string> CreateJobAsync(HttpClient client, string uploadId, string filename)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v1/jobs",
            new { uploadId, options = new { filename } });
        response.EnsureSuccessStatusCode();
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        var jobId = payload.RootElement.GetProperty("jobId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(jobId));
        return jobId!;
    }

    private static async Task WaitForDoneAsync(HttpClient client, string jobId)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(90);
        while (DateTimeOffset.UtcNow < deadline)
        {
            using var response = await client.GetAsync($"/api/v1/jobs/{jobId}");
            response.EnsureSuccessStatusCode();
            using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var status = payload!.RootElement.GetProperty("status").GetString();
            if (status == "done")
            {
                return;
            }

            Assert.NotEqual("failed", status);
            await Task.Delay(250);
        }

        Assert.Fail($"job {jobId} did not reach 'done' within the timeout");
    }

    private sealed class StagingApiFactory(SpyObjectStorage storage) : WebApplicationFactory<MdReel.Api.Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["PipelineModel:Mode"] = "fake",
                    ["PipelineModel:StageRawUploadsToObjectStorage"] = "true",
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IObjectStorage>();
                services.AddSingleton<IObjectStorage>(storage);
            });

            return base.CreateHost(builder);
        }
    }

    private sealed record StorageOp(string Bucket, string ObjectName, long ByteCount);

    // Records writes/deletes; keeps the bytes only long enough to count them (the fake path never
    // reads the staged object back, so an in-memory sink is enough).
    private sealed class SpyObjectStorage : IObjectStorage
    {
        public ConcurrentBag<StorageOp> Writes { get; } = [];

        public ConcurrentBag<StorageOp> Deletes { get; } = [];

        public async Task WriteAsync(string bucket, string objectName, Stream content, CancellationToken cancellationToken)
        {
            using var counter = new CountingStream();
            await content.CopyToAsync(counter, cancellationToken);
            Writes.Add(new StorageOp(bucket, objectName, counter.BytesWritten));
        }

        public Task DeleteAsync(string bucket, string objectName, CancellationToken cancellationToken)
        {
            Deletes.Add(new StorageOp(bucket, objectName, 0));
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string bucket, string objectName, CancellationToken cancellationToken) =>
            Task.FromResult<Stream>(new MemoryStream());

        private sealed class CountingStream : Stream
        {
            public long BytesWritten { get; private set; }

            public override bool CanWrite => true;

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override long Length => BytesWritten;

            public override long Position { get => BytesWritten; set => throw new NotSupportedException(); }

            public override void Write(byte[] buffer, int offset, int count) => BytesWritten += count;

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();
        }
    }
}
